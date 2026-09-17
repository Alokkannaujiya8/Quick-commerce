namespace Catalog.Application.Services;

using BuildingBlocks.Application.Pagination;
using Catalog.Application.DTOs;

public interface ICatalogService
{
    Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<ProductDto>> GetProductsAsync(string? categorySlug, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<ProductDto?> GetProductBySlugAsync(string slug, CancellationToken cancellationToken = default);
}

