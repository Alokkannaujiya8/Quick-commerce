namespace Inventory.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Inventory.Domain.Entities;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options) { }

    public DbSet<DarkStore> DarkStores => Set<DarkStore>();
    public DbSet<StoreInventory> StoreInventories => Set<StoreInventory>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("inventory");
    }
}
