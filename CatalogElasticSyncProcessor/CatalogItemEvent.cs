namespace CatalogElasticSyncProcessor
{
    public class CatalogItemEvent
    {
        public string ProductId { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
