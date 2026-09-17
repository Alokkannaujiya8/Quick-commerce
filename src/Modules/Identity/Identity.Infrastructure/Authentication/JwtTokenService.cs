namespace Identity.Infrastructure.Authentication;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public TokenResponse CreateTokens(ApplicationUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var jwtSection = _configuration.GetSection("Jwt");

        var issuer = jwtSection["Issuer"] ?? "QuickCart";
        var audience = jwtSection["Audience"] ?? "QuickCart.Client";
        var secretKey = jwtSection["SecretKey"] 
            ?? "QuickCart_Default_Jwt_Secret_Key_At_Least_32_Bytes_Long_2026!";

        var accessMinutes = int.TryParse(jwtSection["AccessTokenMinutes"], out var parsedAccess) 
            ? parsedAccess 
            : 15;

        var refreshDays = int.TryParse(jwtSection["RefreshTokenDays"], out var parsedRefresh) 
            ? parsedRefresh 
            : 30;

        var accessExpiresAt = DateTime.UtcNow.AddMinutes(accessMinutes);
        var refreshExpiresAt = DateTime.UtcNow.AddDays(refreshDays);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.MobilePhone, user.PhoneNumber)
        };

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jwtToken = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: accessExpiresAt,
            signingCredentials: credentials
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        return new TokenResponse(accessToken, accessExpiresAt, refreshToken, refreshExpiresAt);
    }
}

