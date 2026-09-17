namespace Promotion.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Promotion.Domain.Entities;

public class PromotionDbContext : DbContext
{
    public PromotionDbContext(DbContextOptions<PromotionDbContext> options) : base(options) { }

    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<Offer> Offers => Set<Offer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("promotion");
    }
}
