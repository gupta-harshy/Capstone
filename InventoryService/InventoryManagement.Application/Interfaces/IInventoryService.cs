using InventoryManagement.Application.DTOs;
using InventoryService.InventoryManagement.Application.DTOs;

namespace InventoryManagement.Application.Interfaces
{
    public interface IInventoryService
    {
        Task<IEnumerable<InventoryDto>> GetAllAsync();
        Task<InventoryDto?> GetByIdAsync(Guid id);
        Task<Guid> CreateAsync(InventoryDto inventory);
        Task UpdateAsync(Guid id, InventoryDto inventory);
        Task DeleteAsync(Guid id);
        Task<ReserveInventoryResultDto> ReserveAsync(IEnumerable<InventoryRequestDto> requests, CancellationToken ct);
        Task ReleaseAsync(IEnumerable<InventoryRequestDto> requests, CancellationToken ct);
    }
}