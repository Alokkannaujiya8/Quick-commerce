namespace Payment.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Payment.Domain.Entities;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentAuditLog> PaymentAuditLogs => Set<PaymentAuditLog>();
    public DbSet<PaymentWebhookEvent> PaymentWebhookEvents => Set<PaymentWebhookEvent>();
    public DbSet<Wallet> Wallets => Set<Wallet>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("payment");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentDbContext).Assembly);
    }
}
