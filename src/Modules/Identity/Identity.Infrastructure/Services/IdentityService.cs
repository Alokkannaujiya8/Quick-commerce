namespace Identity.Infrastructure.Services;

using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using BuildingBlocks.Application.Exceptions;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Microsoft.Extensions.Logging;

public sealed class IdentityService : IIdentityService
{
    public const string GoogleProvider = "Google";
    public const int MaxOtpVerificationAttempts = 5;

    private sealed record OtpEntry(string Code, DateTime ExpiresAt, int FailedAttempts = 0);

    private static readonly ConcurrentDictionary<string, OtpEntry> _otpStore = new();

    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IGoogleTokenValidator? _googleTokenValidator;
    private readonly ILogger<IdentityService>? _logger;

    public IdentityService(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        IGoogleTokenValidator? googleTokenValidator = null,
        ILogger<IdentityService>? logger = null)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
        _googleTokenValidator = googleTokenValidator;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(
        string fullName,
        string phoneNumber,
        string? email,
        string password,
        string? deviceName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ValidationException(nameof(fullName), "Full name is required.");

        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ValidationException(nameof(phoneNumber), "Phone number is required.");

        ValidatePassword(password);

        phoneNumber = NormalizePhone(phoneNumber);
        email = string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();

        if (await _userRepository.PhoneExistsAsync(phoneNumber, cancellationToken))
        {
            throw new ConflictException("Phone number is already registered.");
        }

        if (email is not null && await _userRepository.EmailExistsAsync(email, cancellationToken))
        {
            throw new ConflictException("Email is already registered.");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new ApplicationUser(Guid.NewGuid(), fullName, phoneNumber, passwordHash, email);

        await _userRepository.AddAsync(user, cancellationToken);
        var tokenResponse = await CreateAndAttachRefreshTokenAsync(user, deviceName, cancellationToken);

        await _userRepository.SaveChangesAsync(cancellationToken);

        return new AuthResponse(MapUser(user), tokenResponse);
    }

    public async Task<AuthResponse> LoginAsync(
        string login,
        string password,
        string? deviceName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(login))
            throw new ValidationException(nameof(login), "Login (phone number or email) is required.");

        if (string.IsNullOrWhiteSpace(password))
            throw new ValidationException(nameof(password), "Password is required.");

        var normalizedLogin = login.Trim();

        ApplicationUser? user;
        if (normalizedLogin.Contains('@'))
        {
            user = await _userRepository.GetByEmailAsync(normalizedLogin.ToLowerInvariant(), cancellationToken);
        }
        else
        {
            user = await _userRepository.GetByPhoneNumberAsync(NormalizePhone(normalizedLogin), cancellationToken);
        }

        if (user is null ||
            string.IsNullOrWhiteSpace(user.PasswordHash) ||
            !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        if (user.Status != UserStatus.Active)
        {
            throw new UnauthorizedAccessException("User account is not active.");
        }

        var tokenResponse = await CreateAndAttachRefreshTokenAsync(user, deviceName, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return new AuthResponse(MapUser(user), tokenResponse);
    }

    public Task<bool> SendOtpAsync(
        string phoneNumber,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ValidationException(nameof(phoneNumber), "Phone number is required.");

        var normalizedPhone = NormalizePhone(phoneNumber);
        if (normalizedPhone.Length != 10 || !normalizedPhone.All(char.IsDigit))
            throw new ValidationException(nameof(phoneNumber), "A valid 10-digit phone number is required.");

        var now = DateTime.UtcNow;
        foreach (var kvp in _otpStore)
        {
            if (kvp.Value.ExpiresAt <= now)
            {
                _otpStore.TryRemove(kvp.Key, out _);
            }
        }

        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString("D6");
        var expiresAt = now.AddMinutes(5);

        _otpStore[normalizedPhone] = new OtpEntry(code, expiresAt, 0);

        _logger?.LogDebug("[DEV OTP] Phone: +91{Phone} | Code: {Code} (Expires in 5 mins)", normalizedPhone, code);

        return Task.FromResult(true);
    }

    public async Task<AuthResponse> VerifyOtpAsync(
        string phoneNumber,
        string code,
        string? deviceName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ValidationException(nameof(phoneNumber), "Phone number is required.");

        if (string.IsNullOrWhiteSpace(code))
            throw new ValidationException(nameof(code), "OTP code is required.");

        var normalizedPhone = NormalizePhone(phoneNumber);
        var trimmedCode = code.Trim();

        if (!_otpStore.TryGetValue(normalizedPhone, out var entry))
        {
            throw new UnauthorizedAccessException("Invalid or expired OTP code.");
        }

        if (DateTime.UtcNow >= entry.ExpiresAt)
        {
            _otpStore.TryRemove(normalizedPhone, out _);
            throw new UnauthorizedAccessException("Invalid or expired OTP code.");
        }

        if (!string.Equals(entry.Code, trimmedCode, StringComparison.Ordinal))
        {
            var nextAttempts = entry.FailedAttempts + 1;
            if (nextAttempts >= MaxOtpVerificationAttempts)
            {
                _otpStore.TryRemove(normalizedPhone, out _);
            }
            else
            {
                _otpStore[normalizedPhone] = entry with { FailedAttempts = nextAttempts };
            }

            throw new UnauthorizedAccessException("Invalid or expired OTP code.");
        }

        _otpStore.TryRemove(normalizedPhone, out _);

        var user = await _userRepository.GetByPhoneNumberAsync(normalizedPhone, cancellationToken);
        if (user is null)
        {
            var randomPasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString("N"));
            var defaultName = $"QuickCart User ({normalizedPhone[^4..]})";
            user = new ApplicationUser(Guid.NewGuid(), defaultName, normalizedPhone, randomPasswordHash, null);
            user.VerifyPhoneNumber();
            await _userRepository.AddAsync(user, cancellationToken);
        }
        else
        {
            if (user.Status != UserStatus.Active)
                throw new UnauthorizedAccessException("User account is not active.");

            if (!user.PhoneNumberVerified)
                user.VerifyPhoneNumber();
        }

        var tokenResponse = await CreateAndAttachRefreshTokenAsync(user, deviceName ?? "Phone OTP", cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return new AuthResponse(MapUser(user), tokenResponse);
    }

    public async Task<AuthResponse> GoogleLoginAsync(
        string idToken,
        string? deviceName = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idToken))
            throw new ValidationException(nameof(idToken), "Google ID token is required.");

        if (_googleTokenValidator is null)
            throw new UnauthorizedAccessException("Google authentication validator is not configured.");

        var googlePayload = await _googleTokenValidator.ValidateAsync(idToken, cancellationToken);

        if (string.IsNullOrWhiteSpace(googlePayload.Subject))
            throw new UnauthorizedAccessException("Google token is missing subject claim.");

        if (string.IsNullOrWhiteSpace(googlePayload.Email))
            throw new UnauthorizedAccessException("Google token is missing email claim.");

        if (!googlePayload.EmailVerified)
            throw new UnauthorizedAccessException("Google email address is not verified.");

        var normalizedEmail = googlePayload.Email.Trim().ToLowerInvariant();
        var providerUserId = googlePayload.Subject.Trim();

        // Case 1: Existing user already linked with this Google Subject ID
        var user = await _userRepository.GetByExternalLoginAsync(
            GoogleProvider,
            providerUserId,
            cancellationToken);

        if (user is not null)
        {
            if (user.Status != UserStatus.Active)
                throw new UnauthorizedAccessException("User account is not active.");

            var linkedLogin = user.ExternalLogins.FirstOrDefault(
                x => x.Provider == GoogleProvider && x.ProviderUserId == providerUserId);
            linkedLogin?.RecordLogin(normalizedEmail);

            user.UpdateExternalProfile(
                googlePayload.GivenName,
                googlePayload.FamilyName,
                googlePayload.PictureUrl,
                googlePayload.Name);

            if (!user.EmailVerified)
                user.VerifyEmail();
        }
        else
        {
            // Guard against duplicate external identity if repository has standalone external login record
            var existingExternal = await _userRepository.FindExternalLoginAsync(
                GoogleProvider,
                providerUserId,
                cancellationToken);
            if (existingExternal is not null)
            {
                throw new ConflictException("This Google account is already linked to another user.");
            }

            // Case 2: Existing QuickCart user with the same verified email
            user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

            if (user is not null)
            {
                if (user.Status != UserStatus.Active)
                    throw new UnauthorizedAccessException("User account is not active.");

                var existingGoogleForUser = user.ExternalLogins.FirstOrDefault(x => x.Provider == GoogleProvider);
                if (existingGoogleForUser is not null &&
                    !string.Equals(existingGoogleForUser.ProviderUserId, providerUserId, StringComparison.Ordinal))
                {
                    throw new ConflictException("User account is already linked to a different Google identity.");
                }

                if (existingGoogleForUser is null)
                {
                    var externalLogin = new ExternalLogin(
                        Guid.NewGuid(),
                        user.Id,
                        GoogleProvider,
                        providerUserId,
                        normalizedEmail);

                    user.AddExternalLogin(externalLogin);
                    await _userRepository.AddExternalLoginAsync(externalLogin, cancellationToken);
                }
                else
                {
                    existingGoogleForUser.RecordLogin(normalizedEmail);
                }

                user.UpdateExternalProfile(
                    googlePayload.GivenName,
                    googlePayload.FamilyName,
                    googlePayload.PictureUrl,
                    googlePayload.Name);

                if (!user.EmailVerified)
                    user.VerifyEmail();
            }
            else
            {
                // Case 3: Completely new Google user
                var displayName = !string.IsNullOrWhiteSpace(googlePayload.Name)
                    ? googlePayload.Name.Trim()
                    : $"{googlePayload.GivenName} {googlePayload.FamilyName}".Trim();

                if (string.IsNullOrWhiteSpace(displayName))
                {
                    displayName = normalizedEmail.Split('@')[0];
                }

                user = ApplicationUser.CreateExternalUser(
                    Guid.NewGuid(),
                    displayName,
                    normalizedEmail,
                    emailVerified: true,
                    firstName: googlePayload.GivenName,
                    lastName: googlePayload.FamilyName,
                    profilePictureUrl: googlePayload.PictureUrl);

                var externalLogin = new ExternalLogin(
                    Guid.NewGuid(),
                    user.Id,
                    GoogleProvider,
                    providerUserId,
                    normalizedEmail);

                user.AddExternalLogin(externalLogin);

                await _userRepository.AddAsync(user, cancellationToken);
                await _userRepository.AddExternalLoginAsync(externalLogin, cancellationToken);
            }
        }

        var tokenResponse = await CreateAndAttachRefreshTokenAsync(
            user,
            deviceName ?? "Google Sign-In",
            cancellationToken);

        await _userRepository.SaveChangesAsync(cancellationToken);

        return new AuthResponse(MapUser(user), tokenResponse);
    }

    public static string? GetDevOtpForTesting(string phoneNumber)
    {
        var normalized = NormalizePhone(phoneNumber);
        return _otpStore.TryGetValue(normalized, out var entry) ? entry.Code : null;
    }

    public async Task<AuthResponse> RefreshTokenAsync(
        string refreshToken,
        string? deviceName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new ValidationException(nameof(refreshToken), "Refresh token is required.");

        var tokenHash = HashToken(refreshToken);
        var user = await _userRepository.GetByRefreshTokenHashAsync(tokenHash, cancellationToken);

        if (user is null)
            throw new UnauthorizedAccessException("Invalid refresh token.");

        var storedToken = user.RefreshTokens.FirstOrDefault(x => x.TokenHash == tokenHash);
        if (storedToken is null || !storedToken.IsActive)
        {
            throw new UnauthorizedAccessException("Refresh token is expired or revoked.");
        }

        if (user.Status != UserStatus.Active)
        {
            throw new UnauthorizedAccessException("User account is not active.");
        }

        // Rotate refresh token
        storedToken.Revoke();
        var tokenResponse = await CreateAndAttachRefreshTokenAsync(user, deviceName, cancellationToken);

        await _userRepository.SaveChangesAsync(cancellationToken);

        return new AuthResponse(MapUser(user), tokenResponse);
    }

    public async Task LogoutAsync(
        Guid userId,
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            return;

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return;

        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            var tokenHash = HashToken(refreshToken);
            var token = user.RefreshTokens.FirstOrDefault(x => x.TokenHash == tokenHash);
            token?.Revoke();

            await _userRepository.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<UserDto?> GetUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        return user is null ? null : MapUser(user);
    }

    private async Task<TokenResponse> CreateAndAttachRefreshTokenAsync(
        ApplicationUser user,
        string? deviceName,
        CancellationToken cancellationToken = default)
    {
        var tokenResponse = _jwtTokenService.CreateTokens(user);
        var refreshTokenHash = HashToken(tokenResponse.RefreshToken);

        var refreshToken = new RefreshToken(
            Guid.NewGuid(),
            user.Id,
            refreshTokenHash,
            tokenResponse.RefreshTokenExpiresAt,
            deviceName
        );

        user.AddRefreshToken(refreshToken);
        await _userRepository.AddRefreshTokenAsync(refreshToken, cancellationToken);
        return tokenResponse;
    }

    private static UserDto MapUser(ApplicationUser user)
    {
        return new UserDto(
            user.Id,
            user.FullName,
            user.PhoneNumber,
            user.Email,
            user.Status.ToString(),
            user.PhoneNumberVerified,
            user.EmailVerified,
            user.FirstName,
            user.LastName,
            user.ProfilePictureUrl
        );
    }

    private static string NormalizePhone(string phoneNumber)
    {
        var cleaned = phoneNumber.Trim()
            .Replace(" ", "")
            .Replace("-", "")
            .Replace("(", "")
            .Replace(")", "");

        if (cleaned.StartsWith("+91") && cleaned.Length > 10)
        {
            cleaned = cleaned.Substring(3);
        }
        else if (cleaned.StartsWith("91") && cleaned.Length == 12)
        {
            cleaned = cleaned.Substring(2);
        }
        else if (cleaned.StartsWith("0") && cleaned.Length == 11)
        {
            cleaned = cleaned.Substring(1);
        }

        return cleaned;
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ValidationException(nameof(password), "Password is required.");

        if (password.Length < 8)
            throw new ValidationException(nameof(password), "Password must contain at least 8 characters.");
    }
}
