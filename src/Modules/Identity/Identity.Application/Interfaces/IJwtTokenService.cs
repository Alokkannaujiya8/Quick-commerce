using System;
using System.Collections.Generic;
using System.Text;
using Identity.Application.DTOs;
using Identity.Domain.Entities;

namespace Identity.Application.Interfaces
{
    public interface IJwtTokenService
    {
        TokenResponse CreateTokens(ApplicationUser user);
    }
}
