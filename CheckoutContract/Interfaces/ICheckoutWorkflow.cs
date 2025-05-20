using Temporalio.Workflows;


namespace CheckoutContract.Interfaces
{
    /// <summary>
    /// Defines the Checkout workflow contract for Temporal.
    /// </summary>
    [Workflow]
    public interface ICheckoutWorkflow
    {
        /// <summary>
        /// Executes the checkout process for a given order.
        /// </summary>
        [WorkflowRun]
        Task RunAsync(CheckoutInput input);
    }

    /// <summary>
    /// Input data for the Checkout workflow.
    /// </summary>
    public record CheckoutInput(Guid OrderId, Guid UserId, decimal TotalAmount);
}
