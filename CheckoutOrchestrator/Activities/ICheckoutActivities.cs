using CheckoutOrchestrator.Entities;
using Temporalio.Activities;

namespace CheckoutOrchestrator.Activities;

public interface ICheckoutActivities
{
    [Activity]
    Task<AuthResponse> GetAuthTokenAsync(Guid userId);

    [Activity]
    Task<OrderResultDto> GetOrderDetailsAsync(Guid orderId);

    [Activity]
    Task ReserveInventoryAsync(List<InventoryRequestDto> items);

    [Activity]
    Task ProcessPaymentAsync(Guid orderId, decimal amount);

    [Activity]
    Task SendOrderConfirmedEventAsync(Guid orderId);

    [Activity]
    Task ClearUserCart(string token);

    [Activity]
    Task ReleaseInventoryAsync(List<InventoryRequestDto> items);
    
    [Activity]
    Task UpdateOrderStatusAsync(Guid orderId, string newStatus);
}
