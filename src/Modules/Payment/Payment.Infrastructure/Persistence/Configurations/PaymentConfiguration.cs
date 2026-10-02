namespace Payment.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.Entities;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments", "payment");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.OrderId).IsRequired();
        builder.Property(p => p.UserId).IsRequired();
        builder.Property(p => p.Amount).HasPrecision(12, 2).IsRequired();
        builder.Property(p => p.RefundedAmount).HasPrecision(12, 2).IsRequired();
        builder.Property(p => p.Currency).HasMaxLength(10).IsRequired();

        builder.Property(p => p.Method)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(p => p.ProviderName).HasMaxLength(50).IsRequired();
        builder.Property(p => p.ProviderOrderId).HasMaxLength(128).IsRequired();
        builder.Property(p => p.ProviderPaymentId).HasMaxLength(128);
        builder.Property(p => p.IdempotencyKey).HasMaxLength(128).IsRequired();
        builder.Property(p => p.FailureReason).HasMaxLength(500);
        builder.Property(p => p.RetryCount).IsRequired();
        builder.Property(p => p.CreatedAt).IsRequired();

        builder.HasIndex(p => p.OrderId).HasDatabaseName("IX_Payments_OrderId");
        builder.HasIndex(p => p.IdempotencyKey).IsUnique().HasDatabaseName("IX_Payments_IdempotencyKey");
        builder.HasIndex(p => p.ProviderOrderId).IsUnique().HasDatabaseName("IX_Payments_ProviderOrderId");

        builder.HasMany(p => p.AuditLogs)
            .WithOne()
            .HasForeignKey(a => a.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.AuditLogs)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class PaymentAuditLogConfiguration : IEntityTypeConfiguration<PaymentAuditLog>
{
    public void Configure(EntityTypeBuilder<PaymentAuditLog> builder)
    {
        builder.ToTable("PaymentAuditLogs", "payment");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.PaymentId).IsRequired();
        builder.Property(a => a.EventType).HasMaxLength(60).IsRequired();
        builder.Property(a => a.PreviousStatus).HasMaxLength(30).IsRequired();
        builder.Property(a => a.NewStatus).HasMaxLength(30).IsRequired();
        builder.Property(a => a.ProviderReference).HasMaxLength(128);
        builder.Property(a => a.Notes).HasMaxLength(500);
        builder.Property(a => a.CreatedAt).IsRequired();

        builder.HasIndex(a => a.PaymentId).HasDatabaseName("IX_PaymentAuditLogs_PaymentId");
    }
}

public sealed class PaymentWebhookEventConfiguration : IEntityTypeConfiguration<PaymentWebhookEvent>
{
    public void Configure(EntityTypeBuilder<PaymentWebhookEvent> builder)
    {
        builder.ToTable("PaymentWebhookEvents", "payment");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.ProviderEventId).HasMaxLength(128).IsRequired();
        builder.Property(w => w.EventType).HasMaxLength(60).IsRequired();
        builder.Property(w => w.PayloadHash).HasMaxLength(128).IsRequired();
        builder.Property(w => w.ProcessedAt).IsRequired();

        builder.HasIndex(w => w.ProviderEventId)
            .IsUnique()
            .HasDatabaseName("IX_PaymentWebhookEvents_ProviderEventId");
    }
}

public sealed class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.ToTable("Wallets", "payment");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.UserId).IsRequired();
        builder.Property(w => w.Balance).HasPrecision(12, 2).IsRequired();
        builder.Property(w => w.Currency).HasMaxLength(10).IsRequired();
        builder.Property(w => w.IsActive).IsRequired();
        builder.Property(w => w.CreatedAt).IsRequired();

        builder.HasIndex(w => w.UserId)
            .IsUnique()
            .HasDatabaseName("IX_Wallets_UserId");
    }
}
