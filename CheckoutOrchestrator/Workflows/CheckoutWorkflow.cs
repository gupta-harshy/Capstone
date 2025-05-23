using Temporalio.Workflows;
using CheckoutOrchestrator.Activities;
using AutoMapper;
using CheckoutOrchestrator.Entities;
using Temporalio.Common;
using Temporalio.Exceptions;
using CheckoutContract.Interfaces;
using CheckoutOrchestrator.Mapping;

namespace CheckoutOrchestrator.Workflows;

[Workflow]
public class CheckoutWorkflow : ICheckoutWorkflow
{
    private readonly IMapper _mapper;

    public CheckoutWorkflow()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<EntityMapper>();
            // if you have other profiles:
            // cfg.AddProfile<AnotherProfile>();
        });

        // 2) (Optional) Validate your mapping configuration at startup
        config.AssertConfigurationIsValid();

        // 3) Create the mapper
        _mapper = config.CreateMapper();
    }

    [WorkflowRun]
    public async Task RunAsync(CheckoutInput input)
    {
        // Fetch the token details
        var authResponse = await Workflow.ExecuteActivityAsync(
            (ICheckoutActivities a) => a.GetAuthTokenAsync(input.UserId),
            new ActivityOptions { StartToCloseTimeout = TimeSpan.FromSeconds(15) }
        );

        // Fetch the full order details
        var order = await Workflow.ExecuteActivityAsync(
            (ICheckoutActivities a) => a.GetOrderDetailsAsync(input.OrderId),
            new ActivityOptions { StartToCloseTimeout = TimeSpan.FromSeconds(15) }
        );

        var inventoryRequests = _mapper.Map<List<InventoryRequestDto>>(order.Items);

        // Reserve inventory with the concrete item list
        var inventoryActivityOption = new ActivityOptions
        {
            StartToCloseTimeout = TimeSpan.FromMinutes(1),
            RetryPolicy = new RetryPolicy
            {
                InitialInterval = TimeSpan.FromSeconds(5),
                BackoffCoefficient = 1.5F,
                MaximumAttempts = 3,
                NonRetryableErrorTypes = new[] { "InventoryReservationFailed" }
            }
        };

        // Update Order Status
        var updateOrderActivityOption = new ActivityOptions
        {
            StartToCloseTimeout = TimeSpan.FromMinutes(1),
            RetryPolicy = new RetryPolicy
            {
                InitialInterval = TimeSpan.FromSeconds(5),
                BackoffCoefficient = 1.5F,
                MaximumAttempts = 10
            }
        };

        try
        {
            await Workflow.ExecuteActivityAsync(
                (ICheckoutActivities a) => a.ReserveInventoryAsync(inventoryRequests), inventoryActivityOption);
        }
        catch (ActivityFailureException af)
        {
            var failure = af.InnerException;
            if (failure is ApplicationFailureException app && app.ErrorType == "InventoryReservationFailed")
            {
                // business rejection: release inventory, then cancel workflow
                await Workflow.ExecuteActivityAsync(
                    (ICheckoutActivities a) => a.UpdateOrderStatusAsync(input.OrderId, OrderStatus.InsufficientInventory.ToString()),
                     updateOrderActivityOption);
            }
            throw;
        }
        

        await Workflow.ExecuteActivityAsync(
            (ICheckoutActivities a) => a.UpdateOrderStatusAsync(input.OrderId, OrderStatus.InventoryReserved.ToString()),
            updateOrderActivityOption
        );

        // in CheckoutWorkflow.RunAsync(...)
        var paymentActivityOption = new ActivityOptions
        {
            StartToCloseTimeout = TimeSpan.FromMinutes(5),
            RetryPolicy = new RetryPolicy
            {
                InitialInterval = TimeSpan.FromSeconds(5),
                BackoffCoefficient = 1.5F,
                MaximumAttempts = 3,
                NonRetryableErrorTypes = new[] { "PaymentFailed" }
            }
        };
        
        try
        {
            await Workflow.ExecuteActivityAsync(
                (ICheckoutActivities a) => a.ProcessPaymentAsync(input.OrderId, order.TotalAmount),
                paymentActivityOption);

            await Workflow.ExecuteActivityAsync(
                (ICheckoutActivities a) => a.UpdateOrderStatusAsync(input.OrderId, OrderStatus.PaymentProcessed.ToString()),
                updateOrderActivityOption);
        }
        catch (ActivityFailureException af)
        {
            // unwrap the root cause
            var failure = af.InnerException;
            await Workflow.ExecuteActivityAsync(
                    (ICheckoutActivities a) => a.ReleaseInventoryAsync(inventoryRequests),
                    new ActivityOptions { StartToCloseTimeout = TimeSpan.FromSeconds(30) }
                );

            if (failure is TimeoutException)
            {
                // payment timed out after 5m + retries → compensate immediately
                await Workflow.ExecuteActivityAsync(
                    (ICheckoutActivities a) => a.UpdateOrderStatusAsync(input.OrderId, OrderStatus.PaymentTimeout.ToString()),
                    updateOrderActivityOption);
            }
            else if (failure is ApplicationFailureException app && app.ErrorType == "PaymentFailed")
            {
                // business rejection: release inventory, then cancel workflow
                await Workflow.ExecuteActivityAsync(
                    (ICheckoutActivities a) => a.UpdateOrderStatusAsync(input.OrderId, OrderStatus.PaymentFailed.ToString()),
                    updateOrderActivityOption);
            }
            // any other error: let it bubble, workflow will retry or fail
            throw;
        }

        // Complete the workflow
        await Workflow.ExecuteActivityAsync(
            (ICheckoutActivities a) => a.UpdateOrderStatusAsync(input.OrderId, OrderStatus.Completed.ToString()),
            updateOrderActivityOption);

        // Send confirmation event
        await Workflow.ExecuteActivityAsync(
            (ICheckoutActivities a) => a.ClearUserCart(authResponse.Token),
            new ActivityOptions { StartToCloseTimeout = TimeSpan.FromSeconds(30) }
        );

        // Send confirmation event
        await Workflow.ExecuteActivityAsync(
            (ICheckoutActivities a) => a.SendOrderConfirmedEventAsync(input.OrderId),
            new ActivityOptions { StartToCloseTimeout = TimeSpan.FromSeconds(30) }
        );

        
    }
}
