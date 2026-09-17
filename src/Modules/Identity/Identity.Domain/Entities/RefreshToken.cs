namespace Identity.Domain.Entities;

public class RefreshToken
{
    private RefreshToken() { }

    public RefreshToken(
        Guid id,
        Guid userId,
        string tokenHash,
        DateTime expiresAt,
        string? deviceName = null
    )
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Refresh token ID is required.");

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID is required.");

        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ArgumentException("Token hash is required.");

        Id = id;
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        DeviceName = deviceName;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public ApplicationUser User { get; private set; } = null!;

    public string TokenHash { get; private set; } = string.Empty;

    public string? DeviceName { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    public DateTime? RevokedAt { get; private set; }

    public bool IsRevoked => RevokedAt.HasValue;

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    public bool IsActive => !IsRevoked && !IsExpired;

    public void Revoke()
    {
        if (!RevokedAt.HasValue)
            RevokedAt = DateTime.UtcNow;
    }
}
