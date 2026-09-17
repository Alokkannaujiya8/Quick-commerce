namespace Delivery.Presentation.Controllers;

using Microsoft.AspNetCore.Mvc;
using Delivery.Application.DTOs;
using Delivery.Application.Services;

[ApiController]
[Route("api/delivery")]
public class DeliveryController : ControllerBase
{
    private readonly IDeliveryService _deliveryService;

    public DeliveryController(IDeliveryService deliveryService)
    {
        _deliveryService = deliveryService;
    }

    [HttpPost("orders/{orderId:guid}/assign")]
    public async Task<ActionResult<DeliveryTrackingDto>> AssignRider(Guid orderId, CancellationToken cancellationToken)
    {
        var tracking = await _deliveryService.AssignRiderAsync(orderId, cancellationToken);
        return Ok(tracking);
    }

    [HttpGet("orders/{orderId:guid}/tracking")]
    public async Task<ActionResult<DeliveryTrackingDto>> GetTracking(Guid orderId, CancellationToken cancellationToken)
    {
        var tracking = await _deliveryService.GetTrackingByOrderIdAsync(orderId, cancellationToken);
        if (tracking is null) return NotFound();
        return Ok(tracking);
    }
}

