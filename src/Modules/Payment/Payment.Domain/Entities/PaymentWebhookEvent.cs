namespace Payment.Domain.Entities;

public class PaymentWebhookEvent
{
    public Guid Id { get; private set; }
    public string ProviderEventId { get; private set; } = string.Empty;
    public string EventType { get; private set; } = string.Empty;
    public Guid? PaymentId { get; private set; }
    public string PayloadHash { get; private set; } = string.Empty;
    public DateTime ProcessedAt { get; private set; }

    private PaymentWebhookEvent() { }

    public static PaymentWebhookEvent Create(
        string providerEventId,
        string eventType,
        Guid? paymentId,
        string payloadHash)
    {
        if (string.IsNullOrWhiteSpace(providerEventId))
            throw new ArgumentException("Provider event ID is required.", nameof(providerEventId));

        return new PaymentWebhookEvent
        {
            Id = Guid.NewGuid(),
            ProviderEventId = providerEventId.Trim(),
            EventType = eventType,
            PaymentId = paymentId,
            PayloadHash = payloadHash,
            ProcessedAt = DateTime.UtcNow
        };
    }
}
