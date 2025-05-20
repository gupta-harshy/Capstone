using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.CheckoutTemporalClient;

public interface ICheckoutWorkflowClient
{
    Task StartCheckoutWorkflowAsync(Order order, CancellationToken ct);
}
