using InventoryManagement.Domain.Entities;
using InventoryService.InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Data
{
    public class InventoryDbContext : DbContext
    {
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options) 
        {
            
        }

        public DbSet<Inventory> Inventories => Set<Inventory>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Inventory>()
              .HasKey(i => i.Id);

            modelBuilder.Entity<Inventory>()
              .Property(i => i.Quantity)
              .IsRequired();

            modelBuilder.Entity<Inventory>()
              .Property(i => i.ProductName)
              .IsRequired();

            base.OnModelCreating(modelBuilder);

        }
    }
}