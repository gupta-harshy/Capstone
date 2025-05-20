namespace InventoryService.InventoryManagement.Domain.Entities
{
    public class InventoryItem
    {
        public Guid ProductId { get; private set; }
        public int AvailableQuantity { get; private set; }

        private InventoryItem() { }  // EF

        public InventoryItem(Guid productId, int qty)
        {
            ProductId = productId;
            AvailableQuantity = qty;
        }

        public void Reserve(int qty)
        {
            if (qty > AvailableQuantity)
                throw new InvalidOperationException("Insufficient stock");
            AvailableQuantity -= qty;
        }

        public void Release(int qty)
        {
            AvailableQuantity += qty;
        }
    }
}
