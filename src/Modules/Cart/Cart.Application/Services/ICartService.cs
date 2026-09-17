namespace Cart.Application.Services;

using Cart.Application.DTOs;

public interface ICartService
{
    Task<CartDto> GetCartAsync(Guid? userId, CancellationToken cancellationToken = default);
    Task<CartDto> AddItemAsync(Guid? userId, AddToCartRequest request, CancellationToken cancellationToken = default);
    Task<CartDto> UpdateQuantityAsync(Guid? userId, Guid productId, int quantity, CancellationToken cancellationToken = default);
    Task<CartDto> RemoveItemAsync(Guid? userId, Guid productId, CancellationToken cancellationToken = default);
    Task ClearCartAsync(Guid? userId, CancellationToken cancellationToken = default);
}

