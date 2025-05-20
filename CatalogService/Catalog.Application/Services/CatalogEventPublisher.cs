using CatalogService.Catalog.Application.Interfaces;
using CatalogService.Catalog.Domain.Events;

namespace CatalogService.Catalog.Application.Services;

public class CatalogEventPublisher : IMessagePublisher
{
    private readonly IRabbitMqPublisher _publisher;

    public CatalogEventPublisher(IRabbitMqPublisher publisher)
    {
        _publisher = publisher;
    }

    public Task PublishCatalogEventAsync(CatalogItemEvent e)
    {
        return _publisher.PublishAsync(e, "catalog-events");
    }
}
