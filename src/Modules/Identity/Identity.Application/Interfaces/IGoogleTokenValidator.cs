namespace Identity.Application.Interfaces;

using Identity.Application.DTOs;

public interface IGoogleTokenValidator
{
    Task<GoogleUserPayload> ValidateAsync(
        string idToken,
        CancellationToken cancellationToken = default);
}
