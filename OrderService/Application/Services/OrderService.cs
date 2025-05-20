using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using OrderService.Domain.Entities;
using OrderService.Domain.Repository;
using OrderService.Domain.APIClient;
using OrderService.Domain.Enum;
using OrderService.Infrastructure.CheckoutTemporalClient;

namespace OrderService.Application.Services;

public class OrderService : IOrderService
{
    private readonly ICartApiClient _cartApi;
    private readonly IOrderRepository _orderRepository;
    private readonly ICheckoutWorkflowClient _workflowClient;

    public OrderService(
        ICartApiClient cartApi,
        IOrderRepository orderRepository,
        ICheckoutWorkflowClient workflowClient)
    {
        _cartApi = cartApi;
        _orderRepository = orderRepository;
        _workflowClient = workflowClient;
    }

    public async Task<Guid> PlaceOrderAsync(HttpContext httpContext, CancellationToken ct)
    {
        var userIdClaim = httpContext.User.FindFirst("userId")?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("Invalid or missing userId in JWT.");

        var cart = await _cartApi.GetCartAsync(ct);
        if (cart?.Items == null || !cart.Items.Any())
            throw new InvalidOperationException("Cart is empty or not found.");

        var items = cart.Items
            .Select(i => new OrderItem(Guid.Parse(i.ProductId), i.Quantity, i.Price))
            .ToList();
        var order = new Order(userId, items);

        await _orderRepository.AddAsync(order, ct);
        await _workflowClient.StartCheckoutWorkflowAsync(order, ct);

        return order.Id;
    }

    public async Task<OrderStatus> GetOrderStatusAsync(Guid orderId, CancellationToken ct)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, ct)
                    ?? throw new KeyNotFoundException($"Order {orderId} not found");
        return order.Status;
    }

    public async Task UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus, CancellationToken ct)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, ct)
                    ?? throw new KeyNotFoundException($"Order {orderId} not found");

        // enforce valid domain transition
        switch (newStatus)
        {
            case OrderStatus.InventoryReserved:
                order.MarkInventoryReserved();
                break;
            case OrderStatus.InsufficientInventory:
                order.MarkInsufficientInventory();
                break;
            case OrderStatus.PaymentProcessed:
                order.MarkPaymentProcessed();
                break;
            case OrderStatus.PaymentFailed:
                order.MarkPaymentFailed();
                break;
            case OrderStatus.PaymentTimeout:
                order.MarkPaymentTimeout();
                break;
            case OrderStatus.Completed:
                order.MarkCompleted();
                break;
            case OrderStatus.Cancelled:
                order.MarkCancelled();
                break;
            default:
                throw new InvalidOperationException($"Cannot transition to {newStatus}");
        }


        await _orderRepository.UpdateAsync(order, ct);
    }

    public async Task<Order?> GetOrderAsync(Guid orderId, CancellationToken ct)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, ct)
                    ?? throw new KeyNotFoundException($"Order {orderId} not found");
        return order;
    }
}
