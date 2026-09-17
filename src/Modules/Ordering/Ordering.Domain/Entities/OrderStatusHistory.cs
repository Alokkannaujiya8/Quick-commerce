namespace Ordering.Domain.Entities;

using Ordering.Domain.Enums;

public class OrderStatusHistory
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public OrderStatus Status { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? Notes { get; set; }
}

