using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Persistence;

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> opts)
        : base(opts) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Order>().HasKey(o => o.Id);
        mb.Entity<Order>()
            .HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey("OrderId")
            .IsRequired();
        mb.Entity<OrderItem>().HasKey(i => i.Id);

        mb.Entity<Order>()
          .Property(o => o.Status)
          .HasConversion<string>()    // or .HasConversion<int>() 
          .IsRequired();
    }
}
