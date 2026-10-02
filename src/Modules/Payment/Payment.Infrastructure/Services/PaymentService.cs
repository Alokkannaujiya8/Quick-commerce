namespace Payment.Infrastructure.Services;

using System.Security.Cryptography;
using System.Text;
using BuildingBlocks.Application.Exceptions;
using Microsoft.Extensions.Logging;
using Ordering.Application.Services;
using Payment.Application.DTOs;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;
using Payment.Domain.Enums;

public sealed class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentGatewayService _gatewayService;
    private readonly IOrderService _orderService;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IPaymentGatewayService gatewayService,
        IOrderService orderService,
        ILogger<PaymentService> logger)
    {
        _paymentRepository = paymentRepository;
        _gatewayService = gatewayService;
        _orderService = orderService;
        _logger = logger;
    }

    public async Task<PaymentIntentResponse> CreatePaymentIntentAsync(
        Guid userId,
        CreatePaymentIntentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.OrderId == Guid.Empty)
            throw new ValidationException(nameof(request.OrderId), "OrderId is required.");

        var method = ParsePaymentMethod(request.PaymentMethod);
        var idempotencyKey = string.IsNullOrWhiteSpace(request.IdempotencyKey)
            ? $"pay-intent-{request.OrderId:N}-{method}"
            : request.IdempotencyKey.Trim();

        // 1. Idempotency check
        var existingByKey = await _paymentRepository.GetByIdempotencyKeyAsync(idempotencyKey, cancellationToken);
        if (existingByKey is not null)
        {
            _logger.LogInformation(
                "Returning idempotent payment intent {PaymentId} for Order {OrderId}",
                existingByKey.Id,
                existingByKey.OrderId);

            return MapToIntentResponse(existingByKey, "qcp_idempotent", null, isIdempotentReplay: true);
        }

        // 2. Prevent duplicate payment for an already-captured order
        var existingByOrder = await _paymentRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);
        if (existingByOrder is not null && existingByOrder.Status == PaymentStatus.Captured)
        {
            throw new ConflictException($"Order '{request.OrderId}' has already been paid.");
        }

        // 3. Server-side Order Verification (NEVER trust frontend amount)
        var order = await _orderService.GetOrderByIdAsync(request.OrderId, cancellationToken)
            ?? throw new NotFoundException("Order", request.OrderId);

        if (userId != Guid.Empty && order.UserId != userId)
        {
            throw new UnauthorizedAccessException("Order does not belong to the current user.");
        }

        if (string.Equals(order.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
        {
            throw new ConflictException($"Cannot initiate payment for cancelled order '{request.OrderId}'.");
        }

        var authoritativeAmount = order.TotalAmount;
        var authoritativeUserId = userId != Guid.Empty ? userId : order.UserId;

        // 4. Create gateway order
        var gatewayOrder = await _gatewayService.CreateGatewayOrderAsync(
            order.Id,
            authoritativeAmount,
            "INR",
            cancellationToken);

        var payment = Payment.Create(
            order.Id,
            authoritativeUserId,
            authoritativeAmount,
            "INR",
            method,
            gatewayOrder.ProviderName,
            gatewayOrder.ProviderOrderId,
            idempotencyKey);

        await _paymentRepository.AddPaymentAsync(payment, cancellationToken);
        await _paymentRepository.SaveChangesAsync(cancellationToken);

        // If CashOnDelivery, confirm order placement immediately while keeping payment Pending until delivery
        if (method == PaymentMethodType.CashOnDelivery)
        {
            await _orderService.UpdateOrderStatusAsync(
                order.Id,
                "Confirmed",
                "Order confirmed for Cash on Delivery.",
                cancellationToken);
        }

        _logger.LogInformation(
            "Created payment intent {PaymentId} (ProviderOrderId: {ProviderOrderId}) for Order {OrderId} with server-verified amount {Amount}",
            payment.Id,
            payment.ProviderOrderId,
            payment.OrderId,
            payment.Amount);

        return MapToIntentResponse(
            payment,
            gatewayOrder.ProviderKeyId,
            gatewayOrder.DevVerificationSignature,
            isIdempotentReplay: false);
    }

    public async Task<PaymentDto> VerifyPaymentAsync(
        Guid userId,
        VerifyPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.PaymentId == Guid.Empty)
            throw new ValidationException(nameof(request.PaymentId), "PaymentId is required.");

        var payment = await _paymentRepository.GetByIdAsync(request.PaymentId, cancellationToken)
            ?? throw new NotFoundException("Payment", request.PaymentId);

        if (userId != Guid.Empty && payment.UserId != userId)
        {
            throw new UnauthorizedAccessException("Payment does not belong to the current user.");
        }

        if (payment.Status == PaymentStatus.Captured)
        {
            return MapToDto(payment);
        }

        if (!string.Equals(payment.ProviderOrderId, request.ProviderOrderId?.Trim(), StringComparison.Ordinal))
        {
            throw new ValidationException(nameof(request.ProviderOrderId), "ProviderOrderId does not match the payment intent.");
        }

        var isValidSignature = _gatewayService.VerifyPaymentSignature(
            request.ProviderOrderId ?? string.Empty,
            request.ProviderPaymentId ?? string.Empty,
            request.ProviderSignature ?? string.Empty);

        if (!isValidSignature)
        {
            var failLog = payment.MarkFailed("Invalid payment signature verification.", request.ProviderPaymentId);
            await _paymentRepository.AddAuditLogAsync(failLog, cancellationToken);
            await _paymentRepository.SaveChangesAsync(cancellationToken);

            _logger.LogWarning(
                "Payment signature verification failed for PaymentId {PaymentId}, OrderId {OrderId}",
                payment.Id,
                payment.OrderId);

            throw new UnauthorizedAccessException("Payment signature verification failed.");
        }

        var captureLog = payment.MarkCaptured(
            request.ProviderPaymentId ?? string.Empty,
            "Payment signature verified via gateway checkout callback.");

        await _paymentRepository.AddAuditLogAsync(captureLog, cancellationToken);
        await _paymentRepository.SaveChangesAsync(cancellationToken);

        await _orderService.UpdateOrderStatusAsync(
            payment.OrderId,
            "Confirmed",
            $"Payment captured ({payment.Method} - Ref: {payment.ProviderPaymentId}).",
            cancellationToken);

        _logger.LogInformation(
            "Payment {PaymentId} verified and captured for Order {OrderId}",
            payment.Id,
            payment.OrderId);

        return MapToDto(payment);
    }

    public async Task<WebhookProcessResult> ProcessWebhookAsync(
        string rawPayload,
        string signatureHeader,
        PaymentWebhookPayload payload,
        CancellationToken cancellationToken = default)
    {
        if (!_gatewayService.VerifyWebhookSignature(rawPayload, signatureHeader))
        {
            _logger.LogWarning("Rejected payment webhook due to invalid HMAC signature. EventId: {EventId}", payload?.EventId);
            throw new UnauthorizedAccessException("Invalid webhook signature.");
        }

        if (payload is null || string.IsNullOrWhiteSpace(payload.EventId))
        {
            throw new ValidationException("EventId", "Webhook EventId is required.");
        }

        // Idempotent webhook check
        if (await _paymentRepository.WebhookEventExistsAsync(payload.EventId.Trim(), cancellationToken))
        {
            _logger.LogInformation("Duplicate webhook event {EventId} ignored idempotently.", payload.EventId);
            return new WebhookProcessResult(true, true, null, "DuplicateIgnored");
        }

        var payment = await _paymentRepository.GetByProviderOrderIdAsync(payload.ProviderOrderId, cancellationToken)
            ?? throw new NotFoundException("Payment", payload.ProviderOrderId);

        // Verify webhook amount matches authoritative payment amount
        if (payload.Amount > 0 && decimal.Round(payload.Amount, 2) != payment.Amount)
        {
            var mismatchLog = payment.MarkFailed(
                $"Webhook amount mismatch: expected {payment.Amount:F2}, received {payload.Amount:F2}.",
                payload.ProviderPaymentId);

            await _paymentRepository.AddAuditLogAsync(mismatchLog, cancellationToken);
            await _paymentRepository.SaveChangesAsync(cancellationToken);
            throw new ConflictException("Webhook payment amount does not match order payment amount.");
        }

        var eventTypeNormalized = payload.EventType?.Trim().ToLowerInvariant() ?? string.Empty;
        if (eventTypeNormalized is "payment.captured" or "payment.succeeded" or "order.paid")
        {
            var providerPaymentId = string.IsNullOrWhiteSpace(payload.ProviderPaymentId)
                ? $"wh_pay_{payload.EventId}"
                : payload.ProviderPaymentId;

            if (payment.Status != PaymentStatus.Captured)
            {
                var captureLog = payment.MarkCaptured(
                    providerPaymentId,
                    $"Captured via verified provider webhook ({payload.EventId}).");
                await _paymentRepository.AddAuditLogAsync(captureLog, cancellationToken);
            }

            await _orderService.UpdateOrderStatusAsync(
                payment.OrderId,
                "Confirmed",
                $"Payment confirmed via webhook ({payload.EventId}).",
                cancellationToken);
        }
        else if (eventTypeNormalized is "payment.failed")
        {
            if (payment.Status != PaymentStatus.Captured && payment.Status != PaymentStatus.Failed)
            {
                var failLog = payment.MarkFailed(
                    payload.FailureReason ?? "Payment failed via provider webhook.",
                    payload.ProviderPaymentId);
                await _paymentRepository.AddAuditLogAsync(failLog, cancellationToken);
            }
        }

        var payloadHash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(rawPayload)));
        var webhookEvent = PaymentWebhookEvent.Create(
            payload.EventId.Trim(),
            payload.EventType ?? "unknown",
            payment.Id,
            payloadHash);

        await _paymentRepository.AddWebhookEventAsync(webhookEvent, cancellationToken);
        await _paymentRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Processed webhook event {EventId} ({EventType}) for Payment {PaymentId}, new status: {Status}",
            payload.EventId,
            payload.EventType,
            payment.Id,
            payment.Status);

        return new WebhookProcessResult(true, false, payment.Id, payment.Status.ToString());
    }

    public async Task<PaymentDto?> GetPaymentByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByOrderIdAsync(orderId, cancellationToken);
        return payment is null ? null : MapToDto(payment);
    }

    public async Task<PaymentDto?> GetPaymentByIdAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        return payment is null ? null : MapToDto(payment);
    }

    public async Task<PaymentIntentResponse> RetryPaymentAsync(
        Guid userId,
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken)
            ?? throw new NotFoundException("Payment", paymentId);

        if (userId != Guid.Empty && payment.UserId != userId)
        {
            throw new UnauthorizedAccessException("Payment does not belong to the current user.");
        }

        if (payment.Status == PaymentStatus.Captured)
            throw new ConflictException("Payment has already been captured and cannot be retried.");

        var gatewayOrder = await _gatewayService.CreateGatewayOrderAsync(
            payment.OrderId,
            payment.Amount,
            payment.Currency,
            cancellationToken);

        var retryLog = payment.MarkRetryInitiated(gatewayOrder.ProviderOrderId);
        await _paymentRepository.AddAuditLogAsync(retryLog, cancellationToken);
        await _paymentRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Payment retry #{RetryCount} initiated for Payment {PaymentId}, new ProviderOrderId {ProviderOrderId}",
            payment.RetryCount,
            payment.Id,
            payment.ProviderOrderId);

        return MapToIntentResponse(
            payment,
            gatewayOrder.ProviderKeyId,
            gatewayOrder.DevVerificationSignature,
            isIdempotentReplay: false);
    }

    public async Task<PaymentDto> RefundPaymentAsync(
        Guid paymentId,
        RefundPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0)
            throw new ValidationException(nameof(request.Amount), "Refund amount must be greater than zero.");

        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken)
            ?? throw new NotFoundException("Payment", paymentId);

        if (payment.Status != PaymentStatus.Captured && payment.Status != PaymentStatus.PartiallyRefunded)
            throw new ConflictException($"Payment in status '{payment.Status}' cannot be refunded.");

        var refundReference = await _gatewayService.ProcessRefundAsync(
            payment.ProviderPaymentId ?? payment.ProviderOrderId,
            request.Amount,
            request.Reason ?? "Customer refund",
            cancellationToken);

        try
        {
            var refundLog = payment.ApplyRefund(request.Amount, refundReference, request.Reason ?? "Customer refund");
            await _paymentRepository.AddAuditLogAsync(refundLog, cancellationToken);
            await _paymentRepository.SaveChangesAsync(cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            throw new ConflictException(ex.Message);
        }

        if (payment.Status == PaymentStatus.Refunded)
        {
            await _orderService.UpdateOrderStatusAsync(
                payment.OrderId,
                "Cancelled",
                $"Order refunded in full (Ref: {refundReference}).",
                cancellationToken);
        }

        _logger.LogInformation(
            "Refunded {RefundAmount} for Payment {PaymentId}. New status: {Status}",
            request.Amount,
            payment.Id,
            payment.Status);

        return MapToDto(payment);
    }

    private static PaymentMethodType ParsePaymentMethod(string? method)
    {
        if (string.IsNullOrWhiteSpace(method))
            return PaymentMethodType.UPI;

        return method.Trim().ToUpperInvariant() switch
        {
            "UPI" => PaymentMethodType.UPI,
            "CARD" or "CREDITCARD" or "DEBITCARD" => PaymentMethodType.Card,
            "WALLET" => PaymentMethodType.Wallet,
            "COD" or "CASHONDELIVERY" => PaymentMethodType.CashOnDelivery,
            _ => throw new ValidationException("PaymentMethod", $"Unsupported payment method '{method}'.")
        };
    }

    private static PaymentIntentResponse MapToIntentResponse(
        Payment payment,
        string providerKeyId,
        string? devSignature,
        bool isIdempotentReplay)
    {
        return new PaymentIntentResponse(
            payment.Id,
            payment.OrderId,
            payment.Amount,
            payment.Currency,
            payment.Method.ToString(),
            payment.Status.ToString(),
            payment.ProviderName,
            payment.ProviderOrderId,
            providerKeyId,
            devSignature,
            isIdempotentReplay,
            payment.CreatedAt);
    }

    private static PaymentDto MapToDto(Payment payment)
    {
        return new PaymentDto(
            payment.Id,
            payment.OrderId,
            payment.UserId,
            payment.Amount,
            payment.RefundedAmount,
            payment.Currency,
            payment.Method.ToString(),
            payment.Status.ToString(),
            payment.ProviderName,
            payment.ProviderOrderId,
            payment.ProviderPaymentId,
            payment.FailureReason,
            payment.RetryCount,
            payment.CreatedAt,
            payment.CompletedAt,
            payment.AuditLogs
                .OrderBy(a => a.CreatedAt)
                .Select(a => new PaymentAuditLogDto(
                    a.Id,
                    a.EventType,
                    a.PreviousStatus,
                    a.NewStatus,
                    a.ProviderReference,
                    a.Notes,
                    a.CreatedAt))
                .ToList());
    }
}
