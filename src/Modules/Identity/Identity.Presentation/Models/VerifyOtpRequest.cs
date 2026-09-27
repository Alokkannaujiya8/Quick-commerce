namespace Identity.Presentation.Models;

public sealed record VerifyOtpRequest(
    string PhoneNumber,
    string Code,
    string? DeviceName = null);
