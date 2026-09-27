namespace Identity.Application.DTOs;

public sealed record GoogleUserPayload(
    string Subject,
    string Email,
    bool EmailVerified,
    string? Name = null,
    string? GivenName = null,
    string? FamilyName = null,
    string? PictureUrl = null
);
