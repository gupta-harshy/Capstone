using Catalog.Domain.Entities;

namespace Catalog.Domain.Repositories;

public interface ICatalogRepository
{
    Task AddAsync(CatalogItem item);
    Task UpdateAsync(CatalogItem item);
    Task<CatalogItem?> GetByIdAsync(string id);
}