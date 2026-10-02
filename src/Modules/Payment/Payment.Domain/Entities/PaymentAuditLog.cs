namespace Payment.Domain.Entities;

public class PaymentAuditLog
{
    public Guid Id { get; private set; }
    public Guid PaymentId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string PreviousStatus { get; private set; } = string.Empty;
    public string NewStatus { get; private set; } = string.Empty;
    public string? ProviderReference { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private PaymentAuditLog() { }

    public static PaymentAuditLog Create(
        Guid paymentId,
        string eventType,
        string previousStatus,
        string newStatus,
        string? providerReference = null,
        string? notes = null)
    {
        return new PaymentAuditLog
        {
            Id = Guid.NewGuid(),
            PaymentId = paymentId,
            EventType = eventType,
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            ProviderReference = providerReference,
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        };
    }
}
