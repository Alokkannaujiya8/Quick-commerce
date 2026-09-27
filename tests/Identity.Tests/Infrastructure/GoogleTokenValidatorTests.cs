namespace Identity.Tests.Infrastructure;

using Identity.Infrastructure.Authentication;
using Microsoft.Extensions.Options;
using Xunit;

public class GoogleTokenValidatorTests
{
    [Fact]
    public async Task ValidateAsync_EmptyToken_ThrowsUnauthorizedAccessException()
    {
        var options = Options.Create(new GoogleAuthenticationOptions
        {
            ClientId = "1234567890-test.apps.googleusercontent.com"
        });
        var validator = new GoogleTokenValidator(options);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            validator.ValidateAsync(""));
    }

    [Fact]
    public async Task ValidateAsync_UnconfiguredClientId_ThrowsUnauthorizedAccessException()
    {
        var options = Options.Create(new GoogleAuthenticationOptions
        {
            ClientId = "YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com"
        });
        var validator = new GoogleTokenValidator(options);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            validator.ValidateAsync("some.jwt.token"));
    }

    [Fact]
    public async Task ValidateAsync_MalformedOrInvalidJwt_ThrowsUnauthorizedAccessException()
    {
        var options = Options.Create(new GoogleAuthenticationOptions
        {
            ClientId = "1234567890-test.apps.googleusercontent.com"
        });
        var validator = new GoogleTokenValidator(options);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            validator.ValidateAsync("invalid.jwt.token"));
    }
}
