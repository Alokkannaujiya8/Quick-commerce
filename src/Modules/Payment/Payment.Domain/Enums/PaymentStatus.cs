namespace Payment.Domain.Enums;

public enum PaymentStatus
{
    Pending = 0,
    Initiated = 1,
    Authorized = 2,
    Captured = 3,
    Failed = 4,
    Refunded = 5,
    PartiallyRefunded = 6,
    Cancelled = 7
}
