namespace Catalog.Presentation.Controllers;

using Microsoft.AspNetCore.Mvc;
using Catalog.Application.DTOs;
using Catalog.Application.Services;

[ApiController]
[Route("api/catalog")]
public class ProductsController : ControllerBase
{
    private readonly ICatalogService _catalogService;

    public ProductsController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpGet("categories")]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetCategories(CancellationToken cancellationToken)
    {
        var categories = await _catalogService.GetCategoriesAsync(cancellationToken);
        return Ok(categories);
    }

    [HttpGet("products")]
    public async Task<ActionResult> GetProducts(
        [FromQuery] string? category,
        [FromQuery] string? q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _catalogService.GetProductsAsync(category, q, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("products/{slug}")]
    public async Task<ActionResult<ProductDto>> GetProductBySlug(string slug, CancellationToken cancellationToken)
    {
        var product = await _catalogService.GetProductBySlugAsync(slug, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }

        return Ok(product);
    }
}

