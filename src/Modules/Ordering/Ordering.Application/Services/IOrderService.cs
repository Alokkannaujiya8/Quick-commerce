namespace Ordering.Application.Services;

using Ordering.Application.DTOs;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(Guid userId, CheckoutRequest request, CancellationToken cancellationToken = default);
    Task<OrderDto?> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderDto>> GetUserOrdersAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<OrderDto?> UpdateOrderStatusAsync(Guid orderId, string newStatus, string? notes = null, CancellationToken cancellationToken = default);
}

