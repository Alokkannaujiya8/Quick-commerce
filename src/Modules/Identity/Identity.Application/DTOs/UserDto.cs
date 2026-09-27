namespace Identity.Application.DTOs;

public sealed record UserDto(
    Guid Id,
    string FullName,
    string PhoneNumber,
    string? Email,
    string Status,
    bool PhoneNumberVerified,
    bool EmailVerified,
    string? FirstName = null,
    string? LastName = null,
    string? ProfilePictureUrl = null
);
