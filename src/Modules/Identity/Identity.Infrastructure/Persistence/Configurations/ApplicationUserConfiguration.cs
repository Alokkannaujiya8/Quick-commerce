namespace Identity.Infrastructure.Persistence.Configurations;

using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class ApplicationUserConfiguration
    : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FullName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.FirstName)
            .HasMaxLength(100);

        builder.Property(x => x.LastName)
            .HasMaxLength(100);

        builder.Property(x => x.ProfilePictureUrl)
            .HasMaxLength(2048);

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(20)
            .IsRequired(false)
            .HasConversion(
                v => string.IsNullOrWhiteSpace(v) ? null : v,
                v => v ?? string.Empty);

        builder.HasIndex(x => x.PhoneNumber)
            .IsUnique();

        builder.Property(x => x.Email)
            .HasMaxLength(320);

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.Property(x => x.PasswordHash)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.PhoneNumberVerified)
            .IsRequired();

        builder.Property(x => x.EmailVerified)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        builder.HasMany(x => x.RefreshTokens)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.RefreshTokens)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.ExternalLogins)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.ExternalLogins)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
