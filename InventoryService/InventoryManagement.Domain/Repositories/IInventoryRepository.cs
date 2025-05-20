using InventoryManagement.Domain.Entities;
using InventoryService.InventoryManagement.Application.DTOs;

namespace InventoryManagement.Domain.Repositories
{
    public interface IInventoryRepository
    {
        Task<IEnumerable<Inventory>> GetAllAsync();
        Task<Inventory?> GetByIdAsync(Guid id);
        Task<Guid> AddAsync(Inventory inventory);
        Task UpdateAsync(Inventory inventory);
        Task DeleteAsync(Guid id);
        Task<ReserveInventoryResultDto> ReserveAsync(IEnumerable<InventoryRequestDto> requests, CancellationToken ct);
        Task ReleaseAsync(IEnumerable<InventoryRequestDto> requests, CancellationToken ct);
        Task<IEnumerable<Inventory>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct);
        Task SaveChangesAsync(CancellationToken ct);
    }
}