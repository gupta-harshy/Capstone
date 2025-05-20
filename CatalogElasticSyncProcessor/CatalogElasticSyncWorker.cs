using System.Text.Json;

namespace CatalogElasticSyncProcessor
{
    public class CatalogSyncWorker : BackgroundService
    {
        private readonly IRabbitMqConsumer _consumer;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IElasticSearchService<CatalogIndexDto> _elasticService;
        private readonly ILogger<CatalogSyncWorker> _logger;
        private readonly IConfiguration _configuration;

        public CatalogSyncWorker(
            IRabbitMqConsumer consumer,
            IHttpClientFactory httpClientFactory,
            IElasticSearchService<CatalogIndexDto> elasticService,
            ILogger<CatalogSyncWorker> logger,
            IConfiguration configuration)
        {
            _consumer = consumer;
            _httpClientFactory = httpClientFactory;
            _elasticService = elasticService;
            _logger = logger;
            _configuration = configuration;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string queueName = _configuration["CatalogSync:QueueName"] ?? "catalog-events";
            string catalogBaseUrl = _configuration["CatalogSync:CatalogBaseUrl"] ?? "http://catalog-service:8080";
            string indexName = _configuration["CatalogSync:ElasticIndexName"] ?? "catalogitem";

            var client = _httpClientFactory.CreateClient("CatalogService");
            client.BaseAddress = new Uri(catalogBaseUrl);

            _consumer.Subscribe<CatalogItemEvent>(queueName, async message =>
            {
                try
                {
                    _logger.LogInformation("Received event: {EventType} for ProductId: {ProductId}", message.EventType, message.ProductId);

                    var response = await client.GetAsync($"/api/Catalog/{message.ProductId}");

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Failed to fetch product details for ProductId {Id}", message.ProductId);
                        return;
                    }

                    var json = await response.Content.ReadAsStringAsync();
                    var catalogItem = JsonSerializer.Deserialize<CatalogIndexDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (catalogItem != null)
                    {
                        switch (message.EventType)
                        {
                            case "Deleted":
                                await _elasticService.DeleteDocumentAsync(message.ProductId, indexName);
                                break;

                            case "Updated":
                                await _elasticService.UpdateAsync(message.ProductId, catalogItem, indexName);
                                break;

                            default: // e.g., "Created"
                                await _elasticService.IndexAsync(catalogItem, indexName);
                                break;
                        }

                        _logger.LogInformation("Synced ProductId {ProductId} to Elasticsearch", message.ProductId);
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing CatalogItemEvent for ProductId: {ProductId}", message.ProductId);
                }
            });

            return Task.CompletedTask;
        }
    }
}
