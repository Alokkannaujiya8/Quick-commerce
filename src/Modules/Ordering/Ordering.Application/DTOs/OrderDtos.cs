namespace Ordering.Application.DTOs;

public record OrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string Sku,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal);

public record OrderStatusHistoryDto(
    string Status,
    DateTime ChangedAt,
    string? Notes);

public record OrderDto(
    Guid Id,
    string OrderNumber,
    Guid UserId,
    Guid DeliveryAddressId,
    string Status,
    decimal Subtotal,
    decimal DeliveryFee,
    decimal DiscountAmount,
    decimal TotalAmount,
    DateTime PlacedAt,
    int EstimatedDeliveryMinutes,
    IReadOnlyList<OrderItemDto> Items,
    IReadOnlyList<OrderStatusHistoryDto> StatusHistory);

public record CheckoutItemRequest(
    Guid ProductId,
    string ProductName,
    string? Sku,
    decimal UnitPrice,
    int Quantity);

public record CheckoutRequest(
    Guid DeliveryAddressId,
    string PaymentMethod,
    string? Notes = null,
    IReadOnlyList<CheckoutItemRequest>? Items = null,
    decimal? DeliveryFee = null,
    decimal? DiscountAmount = null);
