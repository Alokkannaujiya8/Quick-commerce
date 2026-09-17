namespace Inventory.Presentation.Controllers;

using Microsoft.AspNetCore.Mvc;
using Inventory.Application.DTOs;
using Inventory.Application.Services;

[ApiController]
[Route("api/inventory")]
public class DarkStoresController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public DarkStoresController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet("dark-stores")]
    public async Task<ActionResult<IReadOnlyList<DarkStoreDto>>> GetDarkStores(CancellationToken cancellationToken)
    {
        var stores = await _inventoryService.GetActiveDarkStoresAsync(cancellationToken);
        return Ok(stores);
    }

    [HttpPost("serviceability")]
    public async Task<ActionResult<ServiceabilityResponse>> CheckServiceability(
        [FromBody] ServiceabilityRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _inventoryService.CheckServiceabilityAsync(request, cancellationToken);
        return Ok(response);
    }
}

