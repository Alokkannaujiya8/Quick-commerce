namespace Delivery.Infrastructure;

using Microsoft.Extensions.DependencyInjection;
using Delivery.Application.Services;
using Delivery.Infrastructure.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddDeliveryInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IDeliveryService, DeliveryService>();
        return services;
    }
}

