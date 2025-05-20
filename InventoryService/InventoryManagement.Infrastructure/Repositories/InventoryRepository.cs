using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Repositories;
using InventoryManagement.Infrastructure.Data;
using InventoryService.InventoryManagement.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Transactions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace InventoryManagement.Infrastructure.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly InventoryDbContext _context;

        public InventoryRepository(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> AddAsync(Inventory inventory)
        {
            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();
            return inventory.Id;
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _context.Inventories.FindAsync(id);
            if (entity != null)
            {
                _context.Inventories.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Inventory>> GetAllAsync() =>
            await _context.Inventories.ToListAsync();

        public async Task<Inventory?> GetByIdAsync(Guid id) =>
            await _context.Inventories.FindAsync(id);

        public async Task<IEnumerable<Inventory>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct) =>
            await _context.Inventories.Where(x => ids.Contains(x.Id)).ToListAsync();

        public Task SaveChangesAsync(CancellationToken ct)
        {
            _context.SaveChangesAsync(ct);
            return Task.CompletedTask;
        }

        public async Task UpdateAsync(Inventory inventory)
        {
            _context.Inventories.Update(inventory);
            await _context.SaveChangesAsync();
        }

        public async Task<ReserveInventoryResultDto> ReserveAsync(IEnumerable<InventoryRequestDto> requests, CancellationToken ct)
        {
            var result = new ReserveInventoryResultDto();

            // 1. Do a quick availability check outside any transaction
            var ids = requests.Select(r => r.ProductId).Distinct().ToList();
            var current = await _context.Inventories
                                   .Where(i => ids.Contains(i.Id))
                                   .ToDictionaryAsync(i => i.Id, i => i.Quantity, ct);

            foreach (var req in requests)
            {
                var avail = current.GetValueOrDefault(req.ProductId, 0);
                if (avail < req.Quantity)
                {
                    result.Shortages.Add(new InventoryShortageDto
                    {
                        ProductId = req.ProductId,
                        RequestedQty = req.Quantity,
                        AvailableQty = avail
                    });
                }
            }

            if (result.Shortages.Any())
            {
                result.Success = false;
                return result;
            }

            /*
            Yes—you can switch from a big “all - or - nothing” transaction to an application-level compensation(a mini - Saga) that:
            Performs each reserve as its own, short‐lived update, Tracks which ones succeeded
              If any update would drive quantity negative, you stop, record the shortage, 
                and then compensate by releasing(adding back) all the prior successful reservations
            */

            var succeeded = new List<InventoryRequestDto>();

            foreach (var req in requests)
            {
                // 1) Try to decrement the quantity if enough remains
                var rows = await _context.Database.ExecuteSqlInterpolatedAsync($@"
                    UPDATE Inventories
                       SET Quantity = Quantity - {req.Quantity}
                     WHERE Id = {req.ProductId}
                       AND Quantity >= {req.Quantity}",
                            cancellationToken: ct);

                if (rows == 1)
                {
                    // reservation succeeded
                    succeeded.Add(req);
                }
                else
                {
                    // shortage: fetch current value for reporting
                    var avail = await _context.Inventories
                                        .Where(i => i.Id == req.ProductId)
                                        .Select(i => i.Quantity)
                                        .FirstOrDefaultAsync(ct);

                    result.Shortages.Add(new InventoryShortageDto
                    {
                        ProductId = req.ProductId,
                        RequestedQty = req.Quantity,
                        AvailableQty = avail
                    });

                    // stop processing further and jump to compensation
                    break;
                }
            }

            if (result.Shortages.Any())
            {
                // 2) Compensate: undo all prior successful reserves
                foreach (var ok in succeeded)
                {
                    await _context.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE Inventories
                   SET Quantity = Quantity + {ok.Quantity}
                 WHERE Id = {ok.ProductId}",
                        cancellationToken: ct);
                }

                result.Success = false;
                return result;
            }

            // 3) If we made it here, every reservation succeeded
            result.Success = true;
            return result;
        }

        public async Task ReleaseAsync(IEnumerable<InventoryRequestDto> requests, CancellationToken ct)
        {
            await using var tx = await _context.Database.BeginTransactionAsync(ct);

            foreach (var req in requests)
            {
                // simply add back quantity
                await _context.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE Inventories
                   SET Quantity = Quantity + {req.Quantity}
                 WHERE Id = {req.ProductId}", cancellationToken: ct);
            }

            await tx.CommitAsync(ct);
        }

    }
}