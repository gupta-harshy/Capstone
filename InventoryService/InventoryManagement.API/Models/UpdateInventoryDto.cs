namespace InventoryService.InventoryManagement.API.Models
{
    public class UpdateInventoryDto
    {
        public string ProductName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
