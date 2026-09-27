namespace Identity.Application.Interfaces;

using Identity.Application.DTOs;

public interface IIdentityService
{
    Task<AuthResponse> RegisterAsync(
        string fullName,
        string phoneNumber,
        string? email,
        string password,
        string? deviceName,
        CancellationToken cancellationToken = default);

    Task<AuthResponse> LoginAsync(
        string login,
        string password,
        string? deviceName,
        CancellationToken cancellationToken = default);

    Task<bool> SendOtpAsync(
        string phoneNumber,
        CancellationToken cancellationToken = default);

    Task<AuthResponse> VerifyOtpAsync(
        string phoneNumber,
        string code,
        string? deviceName,
        CancellationToken cancellationToken = default);

    Task<AuthResponse> GoogleLoginAsync(
        string idToken,
        string? deviceName = null,
        CancellationToken cancellationToken = default);

    Task<AuthResponse> RefreshTokenAsync(
        string refreshToken,
        string? deviceName,
        CancellationToken cancellationToken = default);

    Task LogoutAsync(
        Guid userId,
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task<UserDto?> GetUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
