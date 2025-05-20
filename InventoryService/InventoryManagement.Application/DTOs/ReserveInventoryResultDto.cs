namespace InventoryService.InventoryManagement.Application.DTOs
{

    public class ReserveInventoryResultDto
    {
        public bool Success { get; set; }
        public List<InventoryShortageDto> Shortages { get; set; } = new();
    }
}
