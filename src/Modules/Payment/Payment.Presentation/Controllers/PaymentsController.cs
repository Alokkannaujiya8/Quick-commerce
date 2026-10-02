namespace Payment.Presentation.Controllers;

using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.DTOs;
using Payment.Application.Interfaces;

[ApiController]
[Route("api/payments")]
public sealed class PaymentsController : ControllerBase
{
    private static readonly Guid FallbackGuestUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("intents")]
    public async Task<ActionResult<PaymentIntentResponse>> CreatePaymentIntent(
        [FromBody] CreatePaymentIntentRequest request,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        var response = await _paymentService.CreatePaymentIntentAsync(userId, request, cancellationToken);

        if (response.IsIdempotentReplay)
        {
            return Ok(response);
        }

        return CreatedAtAction(nameof(GetPaymentById), new { id = response.PaymentId }, response);
    }

    [HttpPost("verify")]
    public async Task<ActionResult<PaymentDto>> VerifyPayment(
        [FromBody] VerifyPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        var payment = await _paymentService.VerifyPaymentAsync(userId, request, cancellationToken);
        return Ok(payment);
    }

    [AllowAnonymous]
    [HttpPost("webhook")]
    public async Task<ActionResult<WebhookProcessResult>> HandleWebhook(CancellationToken cancellationToken)
    {
        var signatureHeader = Request.Headers["X-Payment-Signature"].FirstOrDefault() ?? string.Empty;

        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var rawPayload = await reader.ReadToEndAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(rawPayload))
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid Webhook Payload",
                Detail = "Webhook request body cannot be empty."
            });
        }

        var payload = JsonSerializer.Deserialize<PaymentWebhookPayload>(
            rawPayload,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (payload is null)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Malformed Webhook Payload",
                Detail = "Unable to deserialize webhook payload."
            });
        }

        var result = await _paymentService.ProcessWebhookAsync(rawPayload, signatureHeader, payload, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PaymentDto>> GetPaymentById(Guid id, CancellationToken cancellationToken)
    {
        var payment = await _paymentService.GetPaymentByIdAsync(id, cancellationToken);
        if (payment is null) return NotFound();
        return Ok(payment);
    }

    [HttpGet("order/{orderId:guid}")]
    public async Task<ActionResult<PaymentDto>> GetPaymentByOrderId(Guid orderId, CancellationToken cancellationToken)
    {
        var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId, cancellationToken);
        if (payment is null) return NotFound();
        return Ok(payment);
    }

    [HttpPost("{id:guid}/retry")]
    public async Task<ActionResult<PaymentIntentResponse>> RetryPayment(Guid id, CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        var response = await _paymentService.RetryPaymentAsync(userId, id, cancellationToken);
        return Ok(response);
    }

    [HttpPost("{id:guid}/refund")]
    public async Task<ActionResult<PaymentDto>> RefundPayment(
        Guid id,
        [FromBody] RefundPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var payment = await _paymentService.RefundPaymentAsync(id, request, cancellationToken);
        return Ok(payment);
    }

    private Guid ResolveUserId()
    {
        var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return Guid.TryParse(claimValue, out var parsed) ? parsed : FallbackGuestUserId;
    }
}
