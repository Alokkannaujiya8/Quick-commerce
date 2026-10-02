namespace Payment.Domain.Entities;

using global::Payment.Domain.Enums;

public class Payment
{
    private readonly List<PaymentAuditLog> _auditLogs = new();

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid UserId { get; private set; }
    public decimal Amount { get; private set; }
    public decimal RefundedAmount { get; private set; }
    public string Currency { get; private set; } = "INR";
    public PaymentMethodType Method { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string ProviderName { get; private set; } = string.Empty;
    public string ProviderOrderId { get; private set; } = string.Empty;
    public string? ProviderPaymentId { get; private set; }
    public string IdempotencyKey { get; private set; } = string.Empty;
    public string? FailureReason { get; private set; }
    public int RetryCount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public IReadOnlyCollection<PaymentAuditLog> AuditLogs => _auditLogs.AsReadOnly();

    private Payment() { }

    public static Payment Create(
        Guid orderId,
        Guid userId,
        decimal amount,
        string currency,
        PaymentMethodType method,
        string providerName,
        string providerOrderId,
        string idempotencyKey)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("OrderId cannot be empty.", nameof(orderId));
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Payment amount must be greater than zero.");
        if (string.IsNullOrWhiteSpace(providerOrderId))
            throw new ArgumentException("ProviderOrderId is required.", nameof(providerOrderId));
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            throw new ArgumentException("IdempotencyKey is required.", nameof(idempotencyKey));

        var now = DateTime.UtcNow;
        var initialStatus = method == PaymentMethodType.CashOnDelivery
            ? PaymentStatus.Pending
            : PaymentStatus.Initiated;

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            UserId = userId,
            Amount = decimal.Round(amount, 2),
            RefundedAmount = 0m,
            Currency = string.IsNullOrWhiteSpace(currency) ? "INR" : currency.Trim().ToUpperInvariant(),
            Method = method,
            Status = initialStatus,
            ProviderName = providerName,
            ProviderOrderId = providerOrderId,
            IdempotencyKey = idempotencyKey,
            RetryCount = 0,
            CreatedAt = now
        };

        payment._auditLogs.Add(PaymentAuditLog.Create(
            payment.Id,
            "PAYMENT_INTENT_CREATED",
            "None",
            initialStatus.ToString(),
            providerOrderId,
            $"Payment intent created for {payment.Currency} {payment.Amount:F2} via {method}."));

        return payment;
    }

    public PaymentAuditLog MarkCaptured(string providerPaymentId, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(providerPaymentId))
            throw new ArgumentException("ProviderPaymentId is required to capture payment.", nameof(providerPaymentId));

        if (Status == PaymentStatus.Captured)
        {
            return PaymentAuditLog.Create(
                Id,
                "PAYMENT_CAPTURE_IDEMPOTENT",
                Status.ToString(),
                Status.ToString(),
                providerPaymentId,
                notes ?? "Payment was already captured.");
        }

        if (Status == PaymentStatus.Refunded || Status == PaymentStatus.Cancelled)
            throw new InvalidOperationException($"Cannot capture payment in status '{Status}'.");

        var prev = Status;
        var now = DateTime.UtcNow;
        Status = PaymentStatus.Captured;
        ProviderPaymentId = providerPaymentId.Trim();
        FailureReason = null;
        UpdatedAt = now;
        CompletedAt = now;

        var log = PaymentAuditLog.Create(
            Id,
            "PAYMENT_CAPTURED",
            prev.ToString(),
            Status.ToString(),
            ProviderPaymentId,
            notes ?? "Payment verified and captured.");

        _auditLogs.Add(log);
        return log;
    }

    public PaymentAuditLog MarkFailed(string reason, string? providerReference = null)
    {
        if (Status == PaymentStatus.Captured || Status == PaymentStatus.Refunded)
            throw new InvalidOperationException($"Cannot mark payment as failed when already in '{Status}' state.");

        var prev = Status;
        Status = PaymentStatus.Failed;
        FailureReason = string.IsNullOrWhiteSpace(reason) ? "Payment failed verification or was declined." : reason.Trim();
        UpdatedAt = DateTime.UtcNow;

        var log = PaymentAuditLog.Create(
            Id,
            "PAYMENT_FAILED",
            prev.ToString(),
            Status.ToString(),
            providerReference ?? ProviderOrderId,
            FailureReason);

        _auditLogs.Add(log);
        return log;
    }

    public PaymentAuditLog MarkRetryInitiated(string newProviderOrderId)
    {
        if (Status == PaymentStatus.Captured || Status == PaymentStatus.Refunded)
            throw new InvalidOperationException($"Cannot retry payment in status '{Status}'.");
        if (string.IsNullOrWhiteSpace(newProviderOrderId))
            throw new ArgumentException("New ProviderOrderId is required for retry.", nameof(newProviderOrderId));

        var prev = Status;
        ProviderOrderId = newProviderOrderId.Trim();
        Status = PaymentStatus.Initiated;
        FailureReason = null;
        RetryCount++;
        UpdatedAt = DateTime.UtcNow;

        var log = PaymentAuditLog.Create(
            Id,
            "PAYMENT_RETRY_INITIATED",
            prev.ToString(),
            Status.ToString(),
            ProviderOrderId,
            $"Payment retry #{RetryCount} initiated.");

        _auditLogs.Add(log);
        return log;
    }

    public PaymentAuditLog ApplyRefund(decimal refundAmount, string providerRefundId, string reason)
    {
        if (Status != PaymentStatus.Captured && Status != PaymentStatus.PartiallyRefunded)
            throw new InvalidOperationException($"Only captured payments can be refunded. Current status: '{Status}'.");
        if (refundAmount <= 0)
            throw new ArgumentOutOfRangeException(nameof(refundAmount), "Refund amount must be positive.");

        var remainingRefundable = Amount - RefundedAmount;
        if (refundAmount > remainingRefundable)
            throw new InvalidOperationException($"Refund amount ({refundAmount:F2}) exceeds refundable balance ({remainingRefundable:F2}).");

        var prev = Status;
        RefundedAmount = decimal.Round(RefundedAmount + refundAmount, 2);
        Status = RefundedAmount >= Amount ? PaymentStatus.Refunded : PaymentStatus.PartiallyRefunded;
        UpdatedAt = DateTime.UtcNow;

        var log = PaymentAuditLog.Create(
            Id,
            "PAYMENT_REFUNDED",
            prev.ToString(),
            Status.ToString(),
            providerRefundId,
            $"Refunded {Currency} {refundAmount:F2}. Reason: {reason}");

        _auditLogs.Add(log);
        return log;
    }
}
