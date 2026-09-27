namespace Identity.Infrastructure.Repositories;

using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public sealed class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _context;

    public UserRepository(IdentityDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<ApplicationUser?> GetByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.RefreshTokens)
            .Include(u => u.ExternalLogins)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<ApplicationUser?> GetByPhoneNumberAsync(
        string phoneNumber,
        CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.RefreshTokens)
            .Include(u => u.ExternalLogins)
            .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber, cancellationToken);
    }

    public async Task<ApplicationUser?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.RefreshTokens)
            .Include(u => u.ExternalLogins)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<ApplicationUser?> GetByExternalLoginAsync(
        string provider,
        string providerUserId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.RefreshTokens)
            .Include(u => u.ExternalLogins)
            .FirstOrDefaultAsync(
                u => u.ExternalLogins.Any(el => el.Provider == provider && el.ProviderUserId == providerUserId),
                cancellationToken);
    }

    public async Task<ExternalLogin?> FindExternalLoginAsync(
        string provider,
        string providerUserId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ExternalLogins
            .FirstOrDefaultAsync(
                el => el.Provider == provider && el.ProviderUserId == providerUserId,
                cancellationToken);
    }

    public async Task<ApplicationUser?> GetByRefreshTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.RefreshTokens)
            .Include(u => u.ExternalLogins)
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.TokenHash == tokenHash), cancellationToken);
    }

    public async Task<bool> PhoneExistsAsync(
        string phoneNumber,
        CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AnyAsync(u => u.PhoneNumber == phoneNumber, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task AddAsync(
        ApplicationUser user,
        CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }

    public async Task AddRefreshTokenAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken = default)
    {
        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
    }

    public async Task AddExternalLoginAsync(
        ExternalLogin externalLogin,
        CancellationToken cancellationToken = default)
    {
        await _context.ExternalLogins.AddAsync(externalLogin, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
