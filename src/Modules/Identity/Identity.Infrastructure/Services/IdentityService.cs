namespace Identity.Infrastructure.Services;

using System.Security.Cryptography;
using System.Text;
using BuildingBlocks.Application.Exceptions;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Identity.Domain.Enums;

public sealed class IdentityService : IIdentityService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;

    public IdentityService(IUserRepository userRepository, IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
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

        if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
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
            user.EmailVerified
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

