using Catalog.Application.DTOs;
using CatalogService.Catalog.Application.DTOs;

namespace Catalog.Application.Interfaces;

public interface ICatalogService
{
    Task<string> CreateAsync(CatalogCreateDto dto);
    Task UpdateAsync(CatalogUpdateDto dto);
    Task<CatalogDto?> GetByIdAsync(string id);
    Task<IEnumerable<CatalogDto>> SearchAsync(string keyword);
}