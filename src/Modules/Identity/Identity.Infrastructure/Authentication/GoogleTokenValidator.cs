namespace Identity.Infrastructure.Authentication;

using Google.Apis.Auth;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Microsoft.Extensions.Options;

public sealed class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly GoogleAuthenticationOptions _options;

    public GoogleTokenValidator(IOptions<GoogleAuthenticationOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value ?? new GoogleAuthenticationOptions();
    }

    public async Task<GoogleUserPayload> ValidateAsync(
        string idToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idToken))
        {
            throw new UnauthorizedAccessException("Google ID token is required.");
        }

        if (string.IsNullOrWhiteSpace(_options.ClientId) ||
            _options.ClientId.StartsWith("YOUR_GOOGLE_CLIENT_ID", StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Google Sign-In is not configured with a valid Client ID.");
        }

        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _options.ClientId }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            if (payload is null || string.IsNullOrWhiteSpace(payload.Subject))
            {
                throw new UnauthorizedAccessException("Invalid Google ID token payload.");
            }

            if (payload.Issuer is not ("accounts.google.com" or "https://accounts.google.com"))
            {
                throw new UnauthorizedAccessException("Invalid Google ID token issuer.");
            }

            return new GoogleUserPayload(
                Subject: payload.Subject,
                Email: payload.Email ?? string.Empty,
                EmailVerified: payload.EmailVerified,
                Name: payload.Name,
                GivenName: payload.GivenName,
                FamilyName: payload.FamilyName,
                PictureUrl: payload.Picture
            );
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new UnauthorizedAccessException("Invalid or expired Google ID token.");
        }
    }
}
