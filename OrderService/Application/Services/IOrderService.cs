using Microsoft.AspNetCore.Http;
using OrderService.Domain.Entities;
using OrderService.Domain.Enum;

namespace OrderService.Application.Services;

public interface IOrderService
{
    Task<Guid> PlaceOrderAsync(HttpContext httpContext, CancellationToken ct);
    Task<OrderStatus> GetOrderStatusAsync(Guid orderId, CancellationToken ct);
    Task<Order?> GetOrderAsync(Guid orderId, CancellationToken ct);
    Task UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus, CancellationToken ct);
}
