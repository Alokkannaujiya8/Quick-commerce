using Identity.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Application.Interfaces
{
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
}
