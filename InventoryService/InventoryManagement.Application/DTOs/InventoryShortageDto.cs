namespace InventoryService.InventoryManagement.Application.DTOs
{
    public class InventoryShortageDto
    {
        public Guid ProductId { get; set; }
        public int RequestedQty { get; set; }
        public int AvailableQty { get; set; }   // fetched from DB
    }
}
