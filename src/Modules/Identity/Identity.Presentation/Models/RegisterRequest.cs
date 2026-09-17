using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Presentation.Models
{
    public sealed record RegisterRequest(
    string FullName,
    string PhoneNumber,
    string? Email,
    string Password,
    string? DeviceName);
}
