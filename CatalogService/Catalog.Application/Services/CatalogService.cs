using Catalog.Application.DTOs;
using Catalog.Application.Interfaces;
using Catalog.Domain.Entities;
using Catalog.Domain.Repositories;
using CatalogService.Catalog.Application.DTOs;
using CatalogService.Catalog.Domain.Events;
using CatalogService.Catalog.Application.Interfaces;

namespace Catalog.Application.Services;

public class CatalogService : ICatalogService
{
    private readonly ICatalogRepository _repository;
    private readonly IElasticSearchService<CatalogIndexDto> _elasticSearchService;
    private readonly IRabbitMqPublisher _publisher;
    private readonly IInventoryService _inventoryService;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IConfiguration _configuration;


    public CatalogService(ICatalogRepository repository,
                        IRabbitMqPublisher publisher,
                        IInventoryService inventoryService,
                        IMessagePublisher messagePublisher,
                        IElasticSearchService<CatalogIndexDto> elasticSearchService,
                        IConfiguration configuration)
    {
        _repository = repository;
        _publisher = publisher;
        _inventoryService = inventoryService;
        _messagePublisher = messagePublisher;
        _elasticSearchService = elasticSearchService;
        _configuration = configuration;
    }

    public async Task<string> CreateAsync(CatalogCreateDto dto)
    {
        var productId = await _inventoryService.CreateInventoryAsync(dto.Name, dto.Quantity, dto.Price);
        var item = new CatalogItem(productId, dto.Name, dto.Description, dto.Price, dto.Category);
        await _repository.AddAsync(item);

        await _messagePublisher.PublishCatalogEventAsync(new CatalogItemEvent
        {
            ProductId = item.Id,
            EventType = "Created"
        });
        return item.Id;
    }

    public async Task UpdateAsync(CatalogUpdateDto dto)
    {
        var item = new CatalogItem(dto.ProductId, dto.Name, dto.Description, dto.Price, dto.Category);
        await _repository.UpdateAsync(item);
        await _messagePublisher.PublishCatalogEventAsync(new CatalogItemEvent
        {
            ProductId = item.Id,
            EventType = "Updated"
        });
    }

    public async Task<CatalogDto?> GetByIdAsync(string id)
    {
        var item = await _repository.GetByIdAsync(id);
        return item == null ? null : new CatalogDto
        {
            Id = item.Id,
            ProductId = item.Id,
            Name = item.Name,
            Description = item.Description,
            Price = item.Price,
            Category = item.Category
        };
    }

    public async Task<IEnumerable<CatalogDto>> SearchAsync(string keyword)
    {
        var results = await _elasticSearchService.SearchAsync(keyword, new string[] { "name", "description" }, _configuration["ElasticSearchIndexName"]);
        return results.Select(item => new CatalogDto
        {
            Id = item.Id,
            ProductId = item.Id,
            Name = item.Name,
            Description = item.Description,
            Price = item.Price,
            Category = item.Category
        });
    }

    
}