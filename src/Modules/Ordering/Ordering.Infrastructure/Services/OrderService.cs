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

        List<OrderItem> orderItems;
        if (request.Items is { Count: > 0 })
        {
            orderItems = request.Items
                .Where(i => i.Quantity > 0 && i.UnitPrice >= 0)
                .Select(i => new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ProductId = i.ProductId == Guid.Empty ? Guid.NewGuid() : i.ProductId,
                    ProductNameSnapshot = string.IsNullOrWhiteSpace(i.ProductName) ? "QuickCart Item" : i.ProductName.Trim(),
                    ProductSkuSnapshot = string.IsNullOrWhiteSpace(i.Sku) ? "QC-SKU" : i.Sku.Trim(),
                    UnitPrice = decimal.Round(i.UnitPrice, 2),
                    Quantity = i.Quantity,
                    LineTotal = decimal.Round(i.UnitPrice * i.Quantity, 2)
                })
                .ToList();
        }
        else
        {
            orderItems = new List<OrderItem>
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
            };
        }

        var subtotal = orderItems.Sum(i => i.LineTotal);
        var deliveryFee = request.DeliveryFee.HasValue && request.DeliveryFee.Value >= 0
            ? decimal.Round(request.DeliveryFee.Value, 2)
            : (subtotal >= 199m ? 0.00m : 25.00m);
        var discountAmount = request.DiscountAmount.HasValue && request.DiscountAmount.Value >= 0
            ? decimal.Round(Math.Min(request.DiscountAmount.Value, subtotal), 2)
            : 0.00m;
        var totalAmount = Math.Max(0.01m, decimal.Round(subtotal + deliveryFee - discountAmount, 2));

        var order = new Order
        {
            Id = orderId,
            OrderNumber = orderNumber,
            UserId = userId,
            DeliveryAddressId = request.DeliveryAddressId == Guid.Empty ? Guid.NewGuid() : request.DeliveryAddressId,
            Status = OrderStatus.Placed,
            Subtotal = subtotal,
            DeliveryFee = deliveryFee,
            DiscountAmount = discountAmount,
            TotalAmount = totalAmount,
            PlacedAt = now,
            CreatedAt = now,
            OrderItems = orderItems,
            OrderStatusHistories = new List<OrderStatusHistory>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    Status = OrderStatus.Placed,
                    ChangedAt = now,
                    Notes = string.IsNullOrWhiteSpace(request.Notes)
                        ? $"Order placed by customer ({request.PaymentMethod})."
                        : request.Notes
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
