namespace Catalog.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Application.Pagination;
using Catalog.Application.DTOs;
using Catalog.Application.Services;
using Catalog.Infrastructure.Persistence;

public class CatalogService : ICatalogService
{
    private readonly CatalogDbContext _context;

    public CatalogService(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .Include(c => c.SubCategories.Where(sc => sc.IsActive).OrderBy(sc => sc.DisplayOrder))
            .ToListAsync(cancellationToken);

        return categories.Select(c => new CategoryDto(
            c.Id,
            c.Name,
            c.Slug,
            c.IconUrl,
            c.DisplayOrder,
            c.SubCategories.Select(sc => new SubCategoryDto(
                sc.Id,
                sc.CategoryId,
                sc.Name,
                sc.Slug,
                sc.DisplayOrder
            )).ToList()
        )).ToList();
    }

    public async Task<PagedResult<ProductDto>> GetProductsAsync(
        string? categorySlug,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.SubCategory)
            .ThenInclude(sc => sc.Category)
            .Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(categorySlug) && !categorySlug.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(p => p.SubCategory.Category.Slug == categorySlug);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(searchLower) || (p.Description != null && p.Description.ToLower().Contains(searchLower)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var skip = Math.Max(0, (page - 1) * pageSize);
        var take = Math.Clamp(pageSize, 1, 100);

        var items = await query
            .OrderBy(p => p.Name)
            .Skip(skip)
            .Take(take)
            .Select(p => new ProductDto(
                p.Id,
                p.Name,
                p.Slug,
                p.Description,
                p.SKU,
                p.UnitOfMeasure,
                49.00m, // standard unit price
                55.00m, // original price
                p.ImageUrl,
                p.SubCategory.Category.Slug,
                p.IsActive
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductDto>(items, totalCount, page, take);
    }

    public async Task<ProductDto?> GetProductBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Include(p => p.SubCategory)
            .ThenInclude(sc => sc.Category)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive, cancellationToken);

        if (product is null) return null;

        return new ProductDto(
            product.Id,
            product.Name,
            product.Slug,
            product.Description,
            product.SKU,
            product.UnitOfMeasure,
            49.00m,
            55.00m,
            product.ImageUrl,
            product.SubCategory.Category.Slug,
            product.IsActive
        );
    }
}

