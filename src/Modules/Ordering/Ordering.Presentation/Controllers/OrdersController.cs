namespace Ordering.Presentation.Controllers;

using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Ordering.Application.DTOs;
using Ordering.Application.Services;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private static readonly Guid FallbackGuestUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<OrderDto>> Checkout([FromBody] CheckoutRequest request, CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        var order = await _orderService.CreateOrderAsync(userId, request, cancellationToken);
        return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetMyOrders(CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        var orders = await _orderService.GetUserOrdersAsync(userId, cancellationToken);
        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetOrderById(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetOrderByIdAsync(id, cancellationToken);
        if (order is null) return NotFound();
        return Ok(order);
    }

    [HttpPost("{id:guid}/status")]
    public async Task<ActionResult<OrderDto>> UpdateStatus(
        Guid id,
        [FromQuery] string status,
        [FromQuery] string? notes = null,
        CancellationToken cancellationToken = default)
    {
        var order = await _orderService.UpdateOrderStatusAsync(id, status, notes, cancellationToken);
        if (order is null) return NotFound();
        return Ok(order);
    }

    private Guid ResolveUserId()
    {
        var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return Guid.TryParse(claimValue, out var parsed) ? parsed : FallbackGuestUserId;
    }
}
