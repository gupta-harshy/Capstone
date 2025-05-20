namespace CatalogService.Catalog.Domain.Events
{
    public class CatalogItemEvent
    {
        public string ProductId { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty; // Created, Updated, Deleted
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
