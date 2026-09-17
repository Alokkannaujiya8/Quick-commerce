namespace Cart.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using Cart.Application.DTOs;
using Cart.Application.Services;
using Cart.Domain.Entities;
using Cart.Infrastructure.Persistence;

public class CartService : ICartService
{
    private readonly CartDbContext _context;

    public CartService(CartDbContext context)
    {
        _context = context;
    }

    public async Task<CartDto> GetCartAsync(Guid? userId, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        return MapToDto(cart);
    }

    public async Task<CartDto> AddItemAsync(Guid? userId, AddToCartRequest request, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);

        if (existingItem is not null)
        {
            existingItem.Quantity += request.Quantity;
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                CartId = cart.Id,
                ProductId = request.ProductId,
                Name = request.Name,
                Sku = request.Sku,
                UnitOfMeasure = request.UnitOfMeasure,
                Price = request.Price,
                ImageUrl = request.ImageUrl,
                Quantity = request.Quantity
            });
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return MapToDto(cart);
    }

    public async Task<CartDto> UpdateQuantityAsync(Guid? userId, Guid productId, int quantity, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

        if (item is not null)
        {
            if (quantity <= 0)
            {
                cart.Items.Remove(item);
                _context.CartItems.Remove(item);
            }
            else
            {
                item.Quantity = quantity;
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }

        return MapToDto(cart);
    }

    public async Task<CartDto> RemoveItemAsync(Guid? userId, Guid productId, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

        if (item is not null)
        {
            cart.Items.Remove(item);
            _context.CartItems.Remove(item);
            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }

        return MapToDto(cart);
    }

    public async Task ClearCartAsync(Guid? userId, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        _context.CartItems.RemoveRange(cart.Items);
        cart.Items.Clear();
        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<Cart> GetOrCreateCartAsync(Guid? userId, CancellationToken cancellationToken)
    {
        Cart? cart = null;
        if (userId.HasValue)
        {
            cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId.Value, cancellationToken);
        }

        if (cart is null)
        {
            cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == null, cancellationToken);
        }

        if (cart is null)
        {
            cart = new Cart
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return cart;
    }

    private static CartDto MapToDto(Cart cart)
    {
        var items = cart.Items.Select(i => new CartItemDto(
            i.Id,
            i.ProductId,
            i.Name,
            i.Sku,
            i.UnitOfMeasure,
            i.Price,
            i.ImageUrl,
            i.Quantity,
            i.LineTotal
        )).ToList();

        var subtotal = items.Sum(i => i.LineTotal);
        var deliveryFee = subtotal == 0 ? 0 : subtotal >= 500 ? 0 : 40;
        var total = subtotal + deliveryFee;

        return new CartDto(cart.Id, items, subtotal, deliveryFee, total);
    }
}

