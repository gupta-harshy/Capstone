using OrderService.Domain.Enum;

namespace OrderService.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public List<OrderItem> Items { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public OrderStatus Status { get; private set; }

    private Order() { }

    public Order(Guid userId, List<OrderItem> items)
    {
        UserId = userId;
        Items = items;
        TotalAmount = items.Sum(i => i.TotalPrice);
        Status = OrderStatus.Created;
    }

    public void MarkInventoryReserved()
    {
        Status = OrderStatus.InventoryReserved;
    }

    public void MarkInsufficientInventory()
    {
        Status = OrderStatus.InsufficientInventory;
    }

    public void MarkPaymentProcessed()
    {
        Status = OrderStatus.PaymentProcessed;
    }

    public void MarkPaymentFailed()
    {
        Status = OrderStatus.PaymentFailed;
    }

    public void MarkPaymentTimeout()
    {
        Status = OrderStatus.PaymentTimeout;
    }

    public void MarkCompleted()
    {
        Status = OrderStatus.Completed;
    }

    public void MarkCancelled()
    {
        // Cancel can occur from any state
        Status = OrderStatus.Cancelled;
    }
}
