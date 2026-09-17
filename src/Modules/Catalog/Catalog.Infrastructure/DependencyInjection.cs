namespace Catalog.Infrastructure;

using Microsoft.Extensions.DependencyInjection;
using Catalog.Application.Services;
using Catalog.Infrastructure.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ICatalogService, CatalogService>();
        return services;
    }
}

