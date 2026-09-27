namespace Identity.Domain.Entities;

public sealed class ExternalLogin
{
    private ExternalLogin() { }

    public ExternalLogin(
        Guid id,
        Guid userId,
        string provider,
        string providerUserId,
        string? email = null)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("External login ID is required.", nameof(id));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID is required.", nameof(userId));

        if (string.IsNullOrWhiteSpace(provider))
            throw new ArgumentException("Provider is required.", nameof(provider));

        if (string.IsNullOrWhiteSpace(providerUserId))
            throw new ArgumentException("Provider user ID is required.", nameof(providerUserId));

        Id = id;
        UserId = userId;
        Provider = provider.Trim();
        ProviderUserId = providerUserId.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();
        CreatedAt = DateTime.UtcNow;
        LastLoginAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public ApplicationUser User { get; private set; } = null!;

    public string Provider { get; private set; } = string.Empty;

    public string ProviderUserId { get; private set; } = string.Empty;

    public string? Email { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime LastLoginAt { get; private set; }

    public void RecordLogin(string? email = null)
    {
        if (!string.IsNullOrWhiteSpace(email))
        {
            Email = email.Trim().ToLowerInvariant();
        }

        LastLoginAt = DateTime.UtcNow;
    }
}
