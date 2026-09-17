namespace Cart.Presentation.Controllers;

using Microsoft.AspNetCore.Mvc;
using Cart.Application.DTOs;
using Cart.Application.Services;

[ApiController]
[Route("api/cart")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<ActionResult<CartDto>> GetCart(CancellationToken cancellationToken)
    {
        var cart = await _cartService.GetCartAsync(null, cancellationToken);
        return Ok(cart);
    }

    [HttpPost("items")]
    public async Task<ActionResult<CartDto>> AddItem([FromBody] AddToCartRequest request, CancellationToken cancellationToken)
    {
        var cart = await _cartService.AddItemAsync(null, request, cancellationToken);
        return Ok(cart);
    }

    [HttpPut("items/{productId:guid}")]
    public async Task<ActionResult<CartDto>> UpdateQuantity(
        Guid productId,
        [FromBody] UpdateQuantityRequest request,
        CancellationToken cancellationToken)
    {
        var cart = await _cartService.UpdateQuantityAsync(null, productId, request.Quantity, cancellationToken);
        return Ok(cart);
    }

    [HttpDelete("items/{productId:guid}")]
    public async Task<ActionResult<CartDto>> RemoveItem(Guid productId, CancellationToken cancellationToken)
    {
        var cart = await _cartService.RemoveItemAsync(null, productId, cancellationToken);
        return Ok(cart);
    }

    [HttpDelete]
    public async Task<ActionResult> ClearCart(CancellationToken cancellationToken)
    {
        await _cartService.ClearCartAsync(null, cancellationToken);
        return NoContent();
    }
}

