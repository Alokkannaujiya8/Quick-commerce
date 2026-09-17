using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Application.DTOs
{
    public sealed record AuthResponse(UserDto User, TokenResponse Tokens);
}
