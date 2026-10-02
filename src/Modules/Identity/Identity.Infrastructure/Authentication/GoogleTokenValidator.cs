namespace Identity.Infrastructure.Authentication;

using Google.Apis.Auth;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public sealed class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly GoogleAuthenticationOptions _options;
    private readonly ILogger<GoogleTokenValidator>? _logger;
    private readonly Func<string, GoogleJsonWebSignature.ValidationSettings, Task<GoogleJsonWebSignature.Payload>> _signatureValidator;

    public GoogleTokenValidator(
        IOptions<GoogleAuthenticationOptions> options,
        ILogger<GoogleTokenValidator>? logger = null,
        Func<string, GoogleJsonWebSignature.ValidationSettings, Task<GoogleJsonWebSignature.Payload>>? signatureValidator = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value ?? new GoogleAuthenticationOptions();
        _logger = logger;
        _signatureValidator = signatureValidator ?? ((token, settings) => GoogleJsonWebSignature.ValidateAsync(token, settings));
    }

    public async Task<GoogleUserPayload> ValidateAsync(
        string idToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idToken))
        {
            _logger?.LogWarning("Google ID token validation failed: token is null or empty.");
            throw new UnauthorizedAccessException("Google ID token is required.");
        }

        var configuredClientId = _options.ClientId?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(configuredClientId) ||
            configuredClientId.StartsWith("YOUR_GOOGLE_CLIENT_ID", StringComparison.OrdinalIgnoreCase))
        {
            _logger?.LogError("Google ID token validation failed: Authentication:Google:ClientId is not configured.");
            throw new UnauthorizedAccessException("Google Sign-In is not configured with a valid Client ID.");
        }

        if (_options.AllowDevSimulatedTokens &&
            idToken.StartsWith("dev_google_id_token:", StringComparison.Ordinal))
        {
            var parts = idToken.Split(':', StringSplitOptions.TrimEntries);
            var sub = parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]) ? parts[1] : "google-dev-sub-1001";
            var email = parts.Length > 2 && !string.IsNullOrWhiteSpace(parts[2]) ? parts[2] : "alok.quickcart@gmail.com";
            var givenName = parts.Length > 3 && !string.IsNullOrWhiteSpace(parts[3]) ? parts[3] : "Alok";
            var familyName = parts.Length > 4 && !string.IsNullOrWhiteSpace(parts[4]) ? parts[4] : "Kannaujiya";

            return new GoogleUserPayload(
                Subject: sub,
                Email: email,
                EmailVerified: true,
                Name: $"{givenName} {familyName}".Trim(),
                GivenName: givenName,
                FamilyName: familyName,
                PictureUrl: null
            );
        }

        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { configuredClientId },
                IssuedAtClockTolerance = TimeSpan.FromMinutes(2),
                ExpirationTimeClockTolerance = TimeSpan.FromMinutes(2)
            };

            var payload = await _signatureValidator(idToken.Trim(), settings);

            if (payload is null || string.IsNullOrWhiteSpace(payload.Subject))
            {
                _logger?.LogWarning("Google ID token validation failed: missing payload or subject ('sub') claim.");
                throw new UnauthorizedAccessException("Invalid Google ID token payload.");
            }

            if (payload.Issuer is not ("accounts.google.com" or "https://accounts.google.com"))
            {
                _logger?.LogWarning("Google ID token validation failed: invalid issuer '{Issuer}'.", payload.Issuer);
                throw new UnauthorizedAccessException("Invalid Google ID token issuer.");
            }

            if (!HasMatchingAudience(payload.Audience, configuredClientId))
            {
                _logger?.LogWarning("Google ID token validation failed: token audience does not match configured ClientId.");
                throw new UnauthorizedAccessException("Google ID token audience does not match configured Client ID.");
            }

            if (payload.ExpirationTimeSeconds.HasValue)
            {
                var expiresAt = DateTimeOffset.FromUnixTimeSeconds(payload.ExpirationTimeSeconds.Value);
                if (expiresAt <= DateTimeOffset.UtcNow)
                {
                    _logger?.LogWarning("Google ID token validation failed: token expired at {ExpiresAt}.", expiresAt);
                    throw new UnauthorizedAccessException("Google ID token has expired.");
                }
            }

            if (string.IsNullOrWhiteSpace(payload.Email))
            {
                _logger?.LogWarning("Google ID token validation failed: missing email claim for subject.");
                throw new UnauthorizedAccessException("Google ID token is missing required email claim.");
            }

            if (!payload.EmailVerified)
            {
                _logger?.LogWarning("Google ID token validation failed: Google email is not verified.");
                throw new UnauthorizedAccessException("Google account email address is not verified.");
            }

            return new GoogleUserPayload(
                Subject: payload.Subject.Trim(),
                Email: payload.Email.Trim(),
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
        catch (InvalidJwtException ex)
        {
            _logger?.LogWarning("Google ID token validation rejected by GoogleJsonWebSignature: {Reason}", ex.Message);
            throw new UnauthorizedAccessException("Invalid or expired Google ID token.", ex);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger?.LogWarning(ex, "Google ID token validation failed due to an unexpected token error.");
            throw new UnauthorizedAccessException("Invalid or expired Google ID token.", ex);
        }
    }

    private static bool HasMatchingAudience(object? audience, string expectedClientId)
    {
        if (audience is null)
        {
            return false;
        }

        if (audience is string singleAudience)
        {
            return string.Equals(singleAudience.Trim(), expectedClientId, StringComparison.Ordinal);
        }

        if (audience is IEnumerable<string> audienceList)
        {
            return audienceList.Any(a => string.Equals(a?.Trim(), expectedClientId, StringComparison.Ordinal));
        }

        return false;
    }
}
