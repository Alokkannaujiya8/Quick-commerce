namespace Cart.Infrastructure;

using Microsoft.Extensions.DependencyInjection;
using Cart.Application.Services;
using Cart.Infrastructure.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddCartInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ICartService, CartService>();
        return services;
    }
}

