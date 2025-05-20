using CatalogService.Catalog.Application.Interfaces;
using System.Net.Http;
using System.Text.Json;

namespace CatalogService.Catalog.Application.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly HttpClient _client;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<InventoryService> _logger;

        public InventoryService(ILogger<InventoryService> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _client = httpClientFactory.CreateClient("InventoryService");
        }

        public async Task<string> CreateInventoryAsync(string productName, int quantity, decimal price)
        {
            var response = await _client.PostAsJsonAsync("/api/Inventory", new
            {
                ProductName = productName,
                Quantity = quantity,
                Price = price
            });

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to create inventory item for product: {ProductName}", productName);
                throw new Exception("Inventory creation failed.");
            }

            using var stream = await response.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);
            var id = doc.RootElement.GetProperty("id").GetString();

            return id ?? throw new Exception("ProductId not returned.");
        }

        private class InventoryCreatedResponse
        {
            public string Id { get; set; } = string.Empty;
        }
    }
}
