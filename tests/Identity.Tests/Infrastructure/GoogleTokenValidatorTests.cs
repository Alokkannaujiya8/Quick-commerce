namespace Identity.Tests.Infrastructure;

using Google.Apis.Auth;
using Identity.Infrastructure.Authentication;
using Microsoft.Extensions.Options;
using Xunit;

public class GoogleTokenValidatorTests
{
    private const string ConfiguredClientId = "687711243888-2vp11c64p7c4otbberjlsd1j0tclg5f6.apps.googleusercontent.com";

    private static IOptions<GoogleAuthenticationOptions> CreateOptions(string clientId = ConfiguredClientId) =>
        Options.Create(new GoogleAuthenticationOptions
        {
            ClientId = clientId,
            AllowDevSimulatedTokens = false
        });

    [Fact]
    public async Task ValidateAsync_ValidTokenPayload_ReturnsGoogleUserPayload()
    {
        var validator = new GoogleTokenValidator(
            CreateOptions(),
            logger: null,
            signatureValidator: (_, _) => Task.FromResult(new GoogleJsonWebSignature.Payload
            {
                Subject = "google-sub-1001",
                Email = "aarav@gmail.com",
                EmailVerified = true,
                Name = "Aarav Verma",
                GivenName = "Aarav",
                FamilyName = "Verma",
                Picture = "https://lh3.googleusercontent.com/a/aarav",
                Issuer = "https://accounts.google.com",
                Audience = ConfiguredClientId,
                ExpirationTimeSeconds = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds()
            }));

        var result = await validator.ValidateAsync("valid.jwt.token");

        Assert.NotNull(result);
        Assert.Equal("google-sub-1001", result.Subject);
        Assert.Equal("aarav@gmail.com", result.Email);
        Assert.True(result.EmailVerified);
        Assert.Equal("Aarav Verma", result.Name);
        Assert.Equal("Aarav", result.GivenName);
        Assert.Equal("Verma", result.FamilyName);
    }

    [Fact]
    public async Task ValidateAsync_EmptyToken_ThrowsUnauthorizedAccessException()
    {
        var validator = new GoogleTokenValidator(CreateOptions());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            validator.ValidateAsync(""));
    }

    [Fact]
    public async Task ValidateAsync_UnconfiguredClientId_ThrowsUnauthorizedAccessException()
    {
        var validator = new GoogleTokenValidator(
            CreateOptions("YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com"));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            validator.ValidateAsync("some.jwt.token"));
    }

    [Fact]
    public async Task ValidateAsync_MalformedOrInvalidJwt_ThrowsUnauthorizedAccessException()
    {
        var validator = new GoogleTokenValidator(CreateOptions());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            validator.ValidateAsync("invalid.jwt.token"));
    }

    [Fact]
    public async Task ValidateAsync_ExpiredToken_ThrowsUnauthorizedAccessException()
    {
        var validator = new GoogleTokenValidator(
            CreateOptions(),
            logger: null,
            signatureValidator: (_, _) => Task.FromResult(new GoogleJsonWebSignature.Payload
            {
                Subject = "google-sub-expired",
                Email = "expired@gmail.com",
                EmailVerified = true,
                Issuer = "https://accounts.google.com",
                Audience = ConfiguredClientId,
                ExpirationTimeSeconds = DateTimeOffset.UtcNow.AddMinutes(-10).ToUnixTimeSeconds()
            }));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            validator.ValidateAsync("expired.jwt.token"));
    }

    [Fact]
    public async Task ValidateAsync_WrongAudience_ThrowsUnauthorizedAccessException()
    {
        var validator = new GoogleTokenValidator(
            CreateOptions(),
            logger: null,
            signatureValidator: (_, _) => Task.FromResult(new GoogleJsonWebSignature.Payload
            {
                Subject = "google-sub-wrong-aud",
                Email = "user@gmail.com",
                EmailVerified = true,
                Issuer = "https://accounts.google.com",
                Audience = "different-client-id.apps.googleusercontent.com",
                ExpirationTimeSeconds = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds()
            }));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            validator.ValidateAsync("wrong-aud.jwt.token"));
    }

    [Fact]
    public async Task ValidateAsync_WrongIssuer_ThrowsUnauthorizedAccessException()
    {
        var validator = new GoogleTokenValidator(
            CreateOptions(),
            logger: null,
            signatureValidator: (_, _) => Task.FromResult(new GoogleJsonWebSignature.Payload
            {
                Subject = "google-sub-wrong-iss",
                Email = "user@gmail.com",
                EmailVerified = true,
                Issuer = "https://malicious-issuer.example.com",
                Audience = ConfiguredClientId,
                ExpirationTimeSeconds = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds()
            }));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            validator.ValidateAsync("wrong-iss.jwt.token"));
    }

    [Fact]
    public async Task ValidateAsync_UnverifiedEmail_ThrowsUnauthorizedAccessException()
    {
        var validator = new GoogleTokenValidator(
            CreateOptions(),
            logger: null,
            signatureValidator: (_, _) => Task.FromResult(new GoogleJsonWebSignature.Payload
            {
                Subject = "google-sub-unverified",
                Email = "unverified@gmail.com",
                EmailVerified = false,
                Issuer = "https://accounts.google.com",
                Audience = ConfiguredClientId,
                ExpirationTimeSeconds = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds()
            }));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            validator.ValidateAsync("unverified-email.jwt.token"));
    }
}
