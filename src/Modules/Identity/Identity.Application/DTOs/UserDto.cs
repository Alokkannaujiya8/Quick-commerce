using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Application.DTOs
{
    public sealed record UserDto(
        Guid Id,
        string FullName,
        string PhoneNumber,
        string? Email,
        string Status,
        bool PhoneNumberVerified,
        bool EmailVerified
    );
}

