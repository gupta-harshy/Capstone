using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Repositories;
using InventoryService.InventoryManagement.Application.DTOs;

namespace InventoryManagement.Application.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _repository;

        public InventoryService(IInventoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<Guid> CreateAsync(InventoryDto inventory)
        {
            var entity = new Inventory(inventory.ProductName, inventory.Quantity);
            return await _repository.AddAsync(entity);
        }

        public async Task DeleteAsync(Guid id) => await _repository.DeleteAsync(id);

        public async Task<IEnumerable<InventoryDto>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            return entities.Select(e => new InventoryDto { Id = e.Id, ProductName = e.ProductName, Quantity = e.Quantity });
        }

        public async Task<InventoryDto?> GetByIdAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return entity == null ? null : new InventoryDto { Id = entity.Id, ProductName = entity.ProductName, Quantity = entity.Quantity };
        }

        public async Task UpdateAsync(Guid id, InventoryDto inventory)
        {
            var entity = new Inventory(inventory.ProductName, inventory.Quantity) { Id = id };
            await _repository.UpdateAsync(entity);
        }

        public async Task<ReserveInventoryResultDto> ReserveAsync(IEnumerable<InventoryRequestDto> requests, CancellationToken ct)
        {
            return await _repository.ReserveAsync(requests, ct);
        }

        public async Task ReleaseAsync(IEnumerable<InventoryRequestDto> requests, CancellationToken ct)
        {
            await _repository.ReleaseAsync(requests, ct);
        } 
    }
}
