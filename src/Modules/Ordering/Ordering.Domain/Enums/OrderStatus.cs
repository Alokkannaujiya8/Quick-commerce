namespace Ordering.Domain.Enums;

public enum OrderStatus
{
    Placed,
    Confirmed,
    Packed,
    OutForDelivery,
    Delivered,
    Cancelled,
    Returned
}