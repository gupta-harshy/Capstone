using CatalogService.Catalog.Domain.Events;

namespace CatalogService.Catalog.Application.Interfaces;

public interface IMessagePublisher
{
    Task PublishCatalogEventAsync(CatalogItemEvent e);
}
