namespace Payment.Application.Interfaces;

using Payment.Domain.Entities;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<Payment?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);
    Task<Payment?> GetByProviderOrderIdAsync(string providerOrderId, CancellationToken cancellationToken = default);
    Task<bool> WebhookEventExistsAsync(string providerEventId, CancellationToken cancellationToken = default);
    Task AddPaymentAsync(Payment payment, CancellationToken cancellationToken = default);
    Task AddAuditLogAsync(PaymentAuditLog auditLog, CancellationToken cancellationToken = default);
    Task AddWebhookEventAsync(PaymentWebhookEvent webhookEvent, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
