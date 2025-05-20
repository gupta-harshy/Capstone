namespace CheckoutOrchestrator.Entities
{
    public enum OrderStatus
    {
        Created,          // just persisted
        InventoryReserved,
        InsufficientInventory,
        PaymentProcessed,
        PaymentFailed,
        PaymentTimeout,
        Completed,        // all steps done
        Cancelled         // compensated / user-cancelled
    }

}
