namespace InventoryService.InventoryManagement.API.Models
{
    public class CreateInventoryDto
    {
        public string ProductName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
