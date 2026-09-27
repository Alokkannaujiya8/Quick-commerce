namespace Identity.Infrastructure.Persistence.Configurations;

using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class ExternalLoginConfiguration
    : IEntityTypeConfiguration<ExternalLogin>
{
    public void Configure(EntityTypeBuilder<ExternalLogin> builder)
    {
        builder.ToTable("ExternalLogins");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Provider)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ProviderUserId)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasMaxLength(320);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.LastLoginAt)
            .IsRequired();

        builder.HasIndex(x => new { x.Provider, x.ProviderUserId })
            .IsUnique();

        builder.HasIndex(x => x.UserId);
    }
}
