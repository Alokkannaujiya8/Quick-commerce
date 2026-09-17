using Identity.Domain.Enums;

namespace Identity.Domain.Entities;

public sealed class ApplicationUser
{
    private readonly List<RefreshToken> _refreshTokens = new List<RefreshToken>();

    private ApplicationUser() { }

    public ApplicationUser(
        Guid id,
        string fullName,
        string phoneNumber,
        string passwordHash,
        string? email = null
    )
    {
        if (id == Guid.Empty)
            throw new ArgumentException("User ID is required");
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name is required");
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number is required");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required");

        Id = id;
        FullName = fullName.Trim();
        PhoneNumber = phoneNumber.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        Status = UserStatus.Active;
        PhoneNumberVerified = false;
        EmailVerified = false;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string? Email { get; private set; }

    public string PasswordHash { get; private set; } = string.Empty;

    public UserStatus Status { get; private set; }

    public bool PhoneNumberVerified { get; private set; }

    public bool EmailVerified { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    public void UpdateProfile(string fullName, string? email)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name is required.");

        FullName = fullName.Trim();

        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();

        UpdatedAt = DateTime.UtcNow;
    }

    public void VerifyPhoneNumber()
    {
        PhoneNumberVerified = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void VerifyEmail()
    {
        EmailVerified = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Block()
    {
        Status = UserStatus.Blocked;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        Status = UserStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Suspend()
    {
        Status = UserStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkDeleted()
    {
        Status = UserStatus.Deleted;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddRefreshToken(RefreshToken refreshToken)
    {
        _refreshTokens.Add(refreshToken);
    }
}
