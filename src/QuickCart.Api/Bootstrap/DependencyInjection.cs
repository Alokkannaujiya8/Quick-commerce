namespace QuickCart.Api.Bootstrap;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BuildingBlocks.Infrastructure.Persistence.Interceptors;
using BuildingBlocks.Presentation.ExceptionHandling;
using Catalog.Infrastructure;
using Catalog.Infrastructure.Persistence;
using Cart.Infrastructure;
using Cart.Infrastructure.Persistence;
using Ordering.Infrastructure;
using Ordering.Infrastructure.Persistence;
using Identity.Infrastructure;
using Identity.Infrastructure.Persistence;
using Inventory.Infrastructure;
using Inventory.Infrastructure.Persistence;
using Delivery.Infrastructure;
using Delivery.Infrastructure.Persistence;
using Payment.Infrastructure.Persistence;
using Promotion.Infrastructure.Persistence;

public static class DependencyInjection
{
    public const string FrontendCorsPolicy = "QuickCartFrontendPolicy";

    public static IServiceCollection AddApiHost(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // 1. Core ASP.NET Core & Presentation
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            });

        services.AddSignalR();
        services.AddOpenApi();

        // 2. Exception Handling & Problem Details (RFC 7807)
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        // 3. CORS Policy for Angular Frontend
        services.AddCors(options =>
        {
            options.AddPolicy(FrontendCorsPolicy, policy =>
            {
                policy.WithOrigins("http://localhost:4200", "http://localhost:3000", "https://localhost:4200")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        // 4. Shared Persistence Interceptor
        services.AddSingleton<AuditableEntitySaveChangesInterceptor>();

        // 5. Database Contexts (PostgreSQL via Npgsql)
        var connectionString = configuration.GetConnectionString("QuickCartDb")
            ?? "Host=localhost;Port=5432;Database=quickcartdb;Username=postgres;Password=Mom@2026";

        services.AddDbContext<CatalogDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddInterceptors(sp.GetRequiredService<AuditableEntitySaveChangesInterceptor>());
        });

        services.AddDbContext<InventoryDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddInterceptors(sp.GetRequiredService<AuditableEntitySaveChangesInterceptor>());
        });

        services.AddDbContext<OrderingDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddInterceptors(sp.GetRequiredService<AuditableEntitySaveChangesInterceptor>());
        });

        services.AddDbContext<CartDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddInterceptors(sp.GetRequiredService<AuditableEntitySaveChangesInterceptor>());
        });

        services.AddDbContext<DeliveryDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddInterceptors(sp.GetRequiredService<AuditableEntitySaveChangesInterceptor>());
        });

        services.AddDbContext<IdentityDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddInterceptors(sp.GetRequiredService<AuditableEntitySaveChangesInterceptor>());
        });

        services.AddDbContext<PaymentDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddInterceptors(sp.GetRequiredService<AuditableEntitySaveChangesInterceptor>());
        });

        services.AddDbContext<PromotionDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddInterceptors(sp.GetRequiredService<AuditableEntitySaveChangesInterceptor>());
        });

        // 6. Module Infrastructure Services
        services.AddCatalogInfrastructure();
        services.AddCartInfrastructure();
        services.AddOrderingInfrastructure();
        services.AddIdentityInfrastructure();
        services.AddIdentityAuthentication(configuration);
        services.AddInventoryInfrastructure();
        services.AddDeliveryInfrastructure();

        return services;
    }
}

