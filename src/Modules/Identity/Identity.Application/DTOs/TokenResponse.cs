using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Application.DTOs
{
    public sealed record TokenResponse(
        string AccessToken,
        DateTime AccessTokenExpiresAt,
        string RefreshToken,
        DateTime RefreshTokenExpiresAt
    );
}
