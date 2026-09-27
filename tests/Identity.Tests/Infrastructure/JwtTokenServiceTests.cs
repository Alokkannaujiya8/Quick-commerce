namespace Identity.Tests.Infrastructure;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Identity.Domain.Entities;
using Identity.Infrastructure.Authentication;
using Microsoft.Extensions.Configuration;
using Xunit;

public class JwtTokenServiceTests
{
    private readonly IConfiguration _configuration;

    public JwtTokenServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"Jwt:Issuer", "QuickCartTest"},
            {"Jwt:Audience", "QuickCartTestClient"},
            {"Jwt:SecretKey", "SuperSecretKeyForTestingAtLeast32BytesLong!"},
            {"Jwt:AccessTokenMinutes", "30"},
            {"Jwt:RefreshTokenDays", "60"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact]
    public void CreateTokens_ReturnsValidTokensAndClaims()
    {
        // Arrange
        var service = new JwtTokenService(_configuration);
        var user = new ApplicationUser(
            Guid.NewGuid(),
            "Alice Smith",
            "+1234567890",
            "hash123",
            "alice@example.com"
        );

        // Act
        var result = service.CreateTokens(user);

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
        Assert.True(result.AccessTokenExpiresAt > DateTime.UtcNow);
        Assert.True(result.RefreshTokenExpiresAt > result.AccessTokenExpiresAt);

        // Validate JWT token claims
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(result.AccessToken);

        Assert.Equal("QuickCartTest", jwt.Issuer);
        Assert.Contains("QuickCartTestClient", jwt.Audiences);

        var subClaim = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
        Assert.Equal(user.Id.ToString(), subClaim);

        var nameClaim = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        Assert.Equal(user.FullName, nameClaim);

        var emailClaim = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        Assert.Equal(user.Email, emailClaim);
    }

    [Fact]
    public void CreateTokens_ExternalGoogleUserWithoutPhone_GeneratesValidJwtAndRefreshToken()
    {
        // Arrange
        var service = new JwtTokenService(_configuration);
        var googleUser = ApplicationUser.CreateExternalUser(
            Guid.NewGuid(),
            "Google User",
            "googleuser@gmail.com",
            emailVerified: true,
            firstName: "Google",
            lastName: "User"
        );

        // Act
        var result = service.CreateTokens(googleUser);

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(result.AccessToken);
        Assert.Equal(googleUser.Id.ToString(), jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal("googleuser@gmail.com", jwt.Claims.First(c => c.Type == ClaimTypes.Email).Value);
        Assert.DoesNotContain(jwt.Claims, c => c.Type == ClaimTypes.MobilePhone);
    }
}
