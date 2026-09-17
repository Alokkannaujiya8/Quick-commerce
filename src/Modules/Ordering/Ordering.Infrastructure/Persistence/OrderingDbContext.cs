namespace Ordering.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Ordering.Domain.Entities;

using Ordering.Domain.Enums;

public class OrderingDbContext : DbContext
{
    public OrderingDbContext(DbContextOptions<OrderingDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("ordering");

        // Order Configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);

            entity.Property(o => o.OrderNumber).IsRequired().HasMaxLength(30);
            entity.HasIndex(o => o.OrderNumber).IsUnique();

            entity.Property(o => o.UserId).IsRequired();
            entity.HasIndex(o => o.UserId);

            entity.Property(o => o.DeliveryAddressId).IsRequired();

            entity.Property(o => o.Status)
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasDefaultValue(OrderStatus.Placed);

            entity.Property(o => o.Subtotal).HasPrecision(10, 2).IsRequired();
            entity.Property(o => o.DeliveryFee).HasPrecision(10, 2).HasDefaultValue(0m);
            entity.Property(o => o.DiscountAmount).HasPrecision(10, 2).HasDefaultValue(0m);
            entity.Property(o => o.TotalAmount).HasPrecision(10, 2).IsRequired();

            entity.Property(o => o.PlacedAt).IsRequired();
            entity.Property(o => o.CancellationReason).HasMaxLength(500);
            entity.Property(o => o.CreatedAt).IsRequired();

            entity.HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(o => o.OrderStatusHistories)
                .WithOne(osh => osh.Order)
                .HasForeignKey(osh => osh.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // OrderItem Configuration
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(oi => oi.Id);

            entity.Property(oi => oi.OrderId).IsRequired();
            entity.HasIndex(oi => oi.OrderId);

            entity.Property(oi => oi.ProductId).IsRequired();

            entity.Property(oi => oi.ProductNameSnapshot).IsRequired().HasMaxLength(300);
            entity.Property(oi => oi.ProductSkuSnapshot).IsRequired().HasMaxLength(100);

            entity.Property(oi => oi.UnitPrice).HasPrecision(10, 2).IsRequired();
            entity.Property(oi => oi.Quantity).IsRequired();
            entity.Property(oi => oi.LineTotal).HasPrecision(10, 2).IsRequired();
        });

        // OrderStatusHistory Configuration
        modelBuilder.Entity<OrderStatusHistory>(entity =>
        {
            entity.HasKey(osh => osh.Id);

            entity.Property(osh => osh.OrderId).IsRequired();
            entity.HasIndex(osh => osh.OrderId);

            entity.Property(osh => osh.Status)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(osh => osh.ChangedAt).IsRequired();
            entity.Property(osh => osh.Notes).HasMaxLength(500);
        });
    }
}
