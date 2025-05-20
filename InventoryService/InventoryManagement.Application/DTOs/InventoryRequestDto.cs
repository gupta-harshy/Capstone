namespace InventoryService.InventoryManagement.Application.DTOs
{
    public class InventoryRequestDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
