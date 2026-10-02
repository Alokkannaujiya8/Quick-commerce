namespace Payment.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;
using Payment.Infrastructure.Persistence;

public sealed class PaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext _context;

    public PaymentRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(p => p.AuditLogs)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(p => p.AuditLogs)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(p => p.OrderId == orderId, cancellationToken);
    }

    public async Task<Payment?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(p => p.AuditLogs)
            .FirstOrDefaultAsync(p => p.IdempotencyKey == idempotencyKey, cancellationToken);
    }

    public async Task<Payment?> GetByProviderOrderIdAsync(string providerOrderId, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(p => p.AuditLogs)
            .FirstOrDefaultAsync(p => p.ProviderOrderId == providerOrderId, cancellationToken);
    }

    public async Task<bool> WebhookEventExistsAsync(string providerEventId, CancellationToken cancellationToken = default)
    {
        return await _context.PaymentWebhookEvents
            .AsNoTracking()
            .AnyAsync(w => w.ProviderEventId == providerEventId, cancellationToken);
    }

    public async Task AddPaymentAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        await _context.Payments.AddAsync(payment, cancellationToken);
    }

    public async Task AddAuditLogAsync(PaymentAuditLog auditLog, CancellationToken cancellationToken = default)
    {
        await _context.PaymentAuditLogs.AddAsync(auditLog, cancellationToken);
    }

    public async Task AddWebhookEventAsync(PaymentWebhookEvent webhookEvent, CancellationToken cancellationToken = default)
    {
        await _context.PaymentWebhookEvents.AddAsync(webhookEvent, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
