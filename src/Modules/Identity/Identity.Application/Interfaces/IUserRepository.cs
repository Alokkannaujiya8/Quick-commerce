namespace Identity.Application.Interfaces;

using Identity.Domain.Entities;

public interface IUserRepository
{
    Task<ApplicationUser?> GetByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    Task<ApplicationUser?> GetByPhoneNumberAsync(
        string phoneNumber,
        CancellationToken cancellationToken = default
    );

    Task<ApplicationUser?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default
    );

    Task<ApplicationUser?> GetByExternalLoginAsync(
        string provider,
        string providerUserId,
        CancellationToken cancellationToken = default
    );

    Task<ExternalLogin?> FindExternalLoginAsync(
        string provider,
        string providerUserId,
        CancellationToken cancellationToken = default
    );

    Task<ApplicationUser?> GetByRefreshTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default
    );

    Task<bool> PhoneExistsAsync(
        string phoneNumber,
        CancellationToken cancellationToken = default
    );

    Task<bool> EmailExistsAsync(
        string email,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(
        ApplicationUser user,
        CancellationToken cancellationToken = default
    );

    Task AddRefreshTokenAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken = default
    );

    Task AddExternalLoginAsync(
        ExternalLogin externalLogin,
        CancellationToken cancellationToken = default
    );

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default
    );
}
