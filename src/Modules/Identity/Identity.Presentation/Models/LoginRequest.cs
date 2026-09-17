using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Presentation.Models
{
    public sealed record LoginRequest(
    string Login,
    string Password,
    string? DeviceName);
}
