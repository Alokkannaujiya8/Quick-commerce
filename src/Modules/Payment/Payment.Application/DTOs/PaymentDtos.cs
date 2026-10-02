namespace Payment.Application.DTOs;

public record CreatePaymentIntentRequest(
    Guid OrderId,
    string PaymentMethod,
    string? IdempotencyKey = null);

public record PaymentIntentResponse(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount,
    string Currency,
    string PaymentMethod,
    string Status,
    string ProviderName,
    string ProviderOrderId,
    string ProviderKeyId,
    string? DevVerificationSignature,
    bool IsIdempotentReplay,
    DateTime CreatedAt);

public record VerifyPaymentRequest(
    Guid PaymentId,
    string ProviderOrderId,
    string ProviderPaymentId,
    string ProviderSignature);

public record PaymentWebhookPayload(
    string EventId,
    string EventType,
    string ProviderOrderId,
    string? ProviderPaymentId,
    decimal Amount,
    string Currency,
    string? FailureReason,
    DateTime OccurredAt);

public record WebhookProcessResult(
    bool Processed,
    bool IsIdempotentReplay,
    Guid? PaymentId,
    string Status);

public record RefundPaymentRequest(
    decimal Amount,
    string Reason);

public record PaymentAuditLogDto(
    Guid Id,
    string EventType,
    string PreviousStatus,
    string NewStatus,
    string? ProviderReference,
    string? Notes,
    DateTime CreatedAt);

public record PaymentDto(
    Guid Id,
    Guid OrderId,
    Guid UserId,
    decimal Amount,
    decimal RefundedAmount,
    string Currency,
    string PaymentMethod,
    string Status,
    string ProviderName,
    string ProviderOrderId,
    string? ProviderPaymentId,
    string? FailureReason,
    int RetryCount,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    IReadOnlyList<PaymentAuditLogDto> AuditLogs);
