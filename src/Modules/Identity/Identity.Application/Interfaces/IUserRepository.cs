using System;
using System.Collections.Generic;
using System.Text;
using Identity.Domain.Entities;

namespace Identity.Application.Interfaces
{
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

        Task<ApplicationUser?> GetByRefreshTokenHashAsync(
            string tokenHash,
            CancellationToken cancellationToken = default
        );

        Task<bool> PhoneExistsAsync(
            string phoneNumber,
            CancellationToken cancellationToken = default
        );

        Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

        Task AddAsync(ApplicationUser user, CancellationToken cancellationToken = default);

        Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
