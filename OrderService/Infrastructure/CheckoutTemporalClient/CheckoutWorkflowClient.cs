using CheckoutContract.Interfaces;
using OrderService.Domain.Entities;
using Temporalio.Client;

namespace OrderService.Infrastructure.CheckoutTemporalClient;

public class CheckoutWorkflowClient : ICheckoutWorkflowClient
{
    private readonly TemporalClient _client;
    public CheckoutWorkflowClient(TemporalClient client) => _client = client;

    public async Task StartCheckoutWorkflowAsync(Order order, CancellationToken ct)
    {
        var opts = new WorkflowOptions
        {
            Id = $"checkout-{order.Id}",
            TaskQueue = "CHECKOUT_TASK_QUEUE"
        };
        var checkOutInput = new CheckoutInput(order.Id, order.UserId, order.TotalAmount);
        var stub = _client.StartWorkflowAsync<ICheckoutWorkflow>(wf => wf.RunAsync(checkOutInput), opts);   
        await stub;
    }
}
