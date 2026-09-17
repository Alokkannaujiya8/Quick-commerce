namespace Inventory.Infrastructure;

using Microsoft.Extensions.DependencyInjection;
using Inventory.Application.Services;
using Inventory.Infrastructure.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddInventoryInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IInventoryService, InventoryService>();
        return services;
    }
}

