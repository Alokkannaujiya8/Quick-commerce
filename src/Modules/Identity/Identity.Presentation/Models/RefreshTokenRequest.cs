using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Presentation.Models
{
    public sealed record RefreshTokenRequest(
    string RefreshToken,
    string? DeviceName);
}
