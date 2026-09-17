namespace Catalog.Application.DTOs;

public record CategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? IconUrl,
    int DisplayOrder,
    IReadOnlyList<SubCategoryDto> SubCategories);

public record SubCategoryDto(
    Guid Id,
    Guid CategoryId,
    string Name,
    string Slug,
    int DisplayOrder);

public record BrandDto(
    Guid Id,
    string Name,
    string? Description,
    string? LogoUrl);

public record ProductDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string Sku,
    string UnitOfMeasure,
    decimal Price,
    decimal? OriginalPrice,
    string? ImageUrl,
    string CategorySlug,
    bool IsActive);

