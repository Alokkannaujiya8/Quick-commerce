namespace Ordering.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using Ordering.Application.DTOs;
using Ordering.Application.Services;
using Ordering.Domain.Entities;
using Ordering.Domain.Enums;
using Ordering.Infrastructure.Persistence;

public class OrderService : IOrderService
{
    private readonly OrderingDbContext _context;

    public OrderService(OrderingDbContext context)
    {
        _context = context;
    }

    public async Task<OrderDto> CreateOrderAsync(Guid userId, CheckoutRequest request, CancellationToken cancellationToken = default)
    {
        var orderId = Guid.NewGuid();
        var orderNumber = $"QC-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
        var now = DateTime.UtcNow;

        var order = new Order
        {
            Id = orderId,
            OrderNumber = orderNumber,
            UserId = userId,
            DeliveryAddressId = request.DeliveryAddressId,
            Status = OrderStatus.Placed,
            Subtotal = 149.00m,
            DeliveryFee = 0.00m,
            DiscountAmount = 0.00m,
            TotalAmount = 149.00m,
            PlacedAt = now,
            CreatedAt = now,
            OrderItems = new List<OrderItem>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ProductId = Guid.NewGuid(),
                    ProductNameSnapshot = "Fresh Farm Whole Milk",
                    ProductSkuSnapshot = "MILK-001",
                    UnitPrice = 34.00m,
                    Quantity = 2,
                    LineTotal = 68.00m
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ProductId = Guid.NewGuid(),
                    ProductNameSnapshot = "Cold Pressed Orange Juice",
                    ProductSkuSnapshot = "BEV-007",
                    UnitPrice = 81.00m,
                    Quantity = 1,
                    LineTotal = 81.00m
                }
            },
            OrderStatusHistories = new List<OrderStatusHistory>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    Status = OrderStatus.Placed,
                    ChangedAt = now,
                    Notes = "Order placed by customer."
                }
            }
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(order);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
            .Include(o => o.OrderStatusHistories)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        return order is null ? null : MapToDto(order);
    }

    public async Task<IReadOnlyList<OrderDto>> GetUserOrdersAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var orders = await _context.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.PlacedAt)
            .Include(o => o.OrderItems)
            .Include(o => o.OrderStatusHistories)
            .ToListAsync(cancellationToken);

        return orders.Select(MapToDto).ToList();
    }

    public async Task<OrderDto?> UpdateOrderStatusAsync(Guid orderId, string newStatusStr, string? notes = null, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.OrderStatusHistories)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order is null) return null;

        if (Enum.TryParse<OrderStatus>(newStatusStr, true, out var newStatus))
        {
            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;

            if (newStatus == OrderStatus.Delivered)
            {
                order.DeliveredAt = DateTime.UtcNow;
            }
            else if (newStatus == OrderStatus.Cancelled)
            {
                order.CancelledAt = DateTime.UtcNow;
                order.CancellationReason = notes;
            }

            order.OrderStatusHistories.Add(new OrderStatusHistory
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Status = newStatus,
                ChangedAt = DateTime.UtcNow,
                Notes = notes
            });

            await _context.SaveChangesAsync(cancellationToken);
        }

        return MapToDto(order);
    }

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto(
            order.Id,
            order.OrderNumber,
            order.UserId,
            order.DeliveryAddressId,
            order.Status.ToString(),
            order.Subtotal,
            order.DeliveryFee,
            order.DiscountAmount,
            order.TotalAmount,
            order.PlacedAt,
            10,
            order.OrderItems.Select(i => new OrderItemDto(
                i.Id,
                i.ProductId,
                i.ProductNameSnapshot,
                i.ProductSkuSnapshot,
                i.UnitPrice,
                i.Quantity,
                i.LineTotal
            )).ToList(),
            order.OrderStatusHistories.OrderBy(h => h.ChangedAt).Select(h => new OrderStatusHistoryDto(
                h.Status.ToString(),
                h.ChangedAt,
                h.Notes
            )).ToList()
        );
    }
}

