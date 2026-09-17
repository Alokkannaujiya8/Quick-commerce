namespace Cart.Application.DTOs;

public record CartItemDto(
    Guid Id,
    Guid ProductId,
    string Name,
    string Sku,
    string UnitOfMeasure,
    decimal Price,
    string? ImageUrl,
    int Quantity,
    decimal LineTotal);

public record CartDto(
    Guid Id,
    IReadOnlyList<CartItemDto> Items,
    decimal Subtotal,
    decimal DeliveryFee,
    decimal TotalAmount);

public record AddToCartRequest(
    Guid ProductId,
    string Name,
    string Sku,
    string UnitOfMeasure,
    decimal Price,
    string? ImageUrl,
    int Quantity);

public record UpdateQuantityRequest(int Quantity);

