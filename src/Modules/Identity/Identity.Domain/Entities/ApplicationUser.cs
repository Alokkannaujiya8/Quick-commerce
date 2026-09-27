using Identity.Domain.Enums;

namespace Identity.Domain.Entities;

public sealed class ApplicationUser
{
    private readonly List<RefreshToken> _refreshTokens = new List<RefreshToken>();
    private readonly List<ExternalLogin> _externalLogins = new List<ExternalLogin>();

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

    public static ApplicationUser CreateExternalUser(
        Guid id,
        string fullName,
        string email,
        bool emailVerified,
        string? firstName = null,
        string? lastName = null,
        string? profilePictureUrl = null)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("User ID is required", nameof(id));
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name is required", nameof(fullName));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));

        return new ApplicationUser
        {
            Id = id,
            FullName = fullName.Trim(),
            FirstName = string.IsNullOrWhiteSpace(firstName) ? null : firstName.Trim(),
            LastName = string.IsNullOrWhiteSpace(lastName) ? null : lastName.Trim(),
            ProfilePictureUrl = string.IsNullOrWhiteSpace(profilePictureUrl) ? null : profilePictureUrl.Trim(),
            PhoneNumber = string.Empty,
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = string.Empty,
            Status = UserStatus.Active,
            PhoneNumberVerified = false,
            EmailVerified = emailVerified,
            CreatedAt = DateTime.UtcNow
        };
    }

    public Guid Id { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string? ProfilePictureUrl { get; private set; }
    public string PhoneNumber { get; private set; } = string.Empty;
    public string? Email { get; private set; }

    public string PasswordHash { get; private set; } = string.Empty;

    public UserStatus Status { get; private set; }

    public bool PhoneNumberVerified { get; private set; }

    public bool EmailVerified { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    public IReadOnlyCollection<ExternalLogin> ExternalLogins => _externalLogins.AsReadOnly();

    public void UpdateProfile(string fullName, string? email)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name is required.");

        FullName = fullName.Trim();

        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();

        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateExternalProfile(
        string? firstName,
        string? lastName,
        string? profilePictureUrl,
        string? displayName = null)
    {
        if (!string.IsNullOrWhiteSpace(firstName))
            FirstName = firstName.Trim();

        if (!string.IsNullOrWhiteSpace(lastName))
            LastName = lastName.Trim();

        if (!string.IsNullOrWhiteSpace(profilePictureUrl))
            ProfilePictureUrl = profilePictureUrl.Trim();

        if (!string.IsNullOrWhiteSpace(displayName) && string.IsNullOrWhiteSpace(FullName))
            FullName = displayName.Trim();

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

    public void AddExternalLogin(ExternalLogin externalLogin)
    {
        ArgumentNullException.ThrowIfNull(externalLogin);
        _externalLogins.Add(externalLogin);
    }
}
