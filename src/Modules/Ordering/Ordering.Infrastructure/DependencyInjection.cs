namespace Ordering.Infrastructure;

using Microsoft.Extensions.DependencyInjection;
using Ordering.Application.Services;
using Ordering.Infrastructure.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderingInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IOrderService, OrderService>();
        return services;
    }
}

