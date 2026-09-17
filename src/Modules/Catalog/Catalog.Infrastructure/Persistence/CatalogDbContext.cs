namespace Catalog.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Catalog.Domain.Entities;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<SubCategory> SubCategories => Set<SubCategory>();
    public DbSet<Brand> Brands => Set<Brand>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("catalog");

        // Brand Configuration
        modelBuilder.Entity<Brand>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Name).IsRequired().HasMaxLength(200);
            entity.Property(b => b.Description).HasMaxLength(1000);
            entity.Property(b => b.LogoUrl).HasMaxLength(500);
            entity.Property(b => b.IsActive).HasDefaultValue(true);
            entity.Property(b => b.CreatedAt).IsRequired();

            entity.HasMany(b => b.Products)
                .WithOne(p => p.Brand)
                .HasForeignKey(p => p.BrandId)
                .IsRequired(false);
        });

        // Category Configuration
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Slug).IsRequired().HasMaxLength(200);
            entity.HasIndex(c => c.Slug).IsUnique();
            entity.Property(c => c.IconUrl).HasMaxLength(500);
            entity.Property(c => c.DisplayOrder).HasDefaultValue(0);
            entity.Property(c => c.IsActive).HasDefaultValue(true);
            entity.Property(c => c.CreatedAt).IsRequired();

            entity.HasMany(c => c.SubCategories)
                .WithOne(sc => sc.Category)
                .HasForeignKey(sc => sc.CategoryId)
                .IsRequired();
        });

        // SubCategory Configuration
        modelBuilder.Entity<SubCategory>(entity =>
        {
            entity.HasKey(sc => sc.Id);
            entity.Property(sc => sc.Name).IsRequired().HasMaxLength(200);
            entity.Property(sc => sc.Slug).IsRequired().HasMaxLength(200);
            entity.HasIndex(sc => sc.Slug).IsUnique();
            entity.Property(sc => sc.DisplayOrder).HasDefaultValue(0);
            entity.Property(sc => sc.IsActive).HasDefaultValue(true);
            entity.Property(sc => sc.CreatedAt).IsRequired();

            entity.HasMany(sc => sc.Products)
                .WithOne(p => p.SubCategory)
                .HasForeignKey(p => p.SubCategoryId)
                .IsRequired();
        });

        // Product Configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(300);
            entity.Property(p => p.Slug).IsRequired().HasMaxLength(300);
            entity.HasIndex(p => p.Slug).IsUnique();
            entity.Property(p => p.Description).HasMaxLength(2000);
            entity.Property(p => p.SKU).IsRequired().HasMaxLength(100);
            entity.HasIndex(p => p.SKU).IsUnique();
            entity.Property(p => p.UnitOfMeasure).IsRequired().HasMaxLength(50);
            entity.Property(p => p.ImageUrl).HasMaxLength(500);
            entity.Property(p => p.IsActive).HasDefaultValue(true);
            entity.Property(p => p.CreatedAt).IsRequired();
        });
    }
}
