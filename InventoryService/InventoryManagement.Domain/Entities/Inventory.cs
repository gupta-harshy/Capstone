using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Domain.Entities
{
    public class Inventory
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ProductName { get; private set; }
        public int Quantity { get; private set; }
        private Inventory() { }
        public Inventory(string productName, int quantity)
        {
            Id = Guid.NewGuid();
            ProductName = productName;
            Quantity = quantity;
        }

        /// <summary>
        /// Reserve qty from stock, or throw if insufficient.
        /// </summary>
        public void Reserve(int qty)
        {
            if (qty <= 0)
                throw new ArgumentException("Quantity must be > 0", nameof(qty));
            if (Quantity < qty)
                throw new InvalidOperationException($"Insufficient stock for '{ProductName}'");
            Quantity -= qty;
        }

        /// <summary>
        /// Release qty back to stock.
        /// </summary>
        public void Release(int qty)
        {
            if (qty <= 0)
                throw new ArgumentException("Quantity must be > 0", nameof(qty));
            Quantity += qty;
        }
    }
}