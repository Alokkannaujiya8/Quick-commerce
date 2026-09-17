using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Catalog.Infrastructure.Persistence;
using Inventory.Infrastructure.Persistence;
using Ordering.Infrastructure.Persistence;
using Cart.Infrastructure.Persistence;
using Delivery.Infrastructure.Persistence;
using Identity.Infrastructure.Persistence;
using Payment.Infrastructure.Persistence;
using Promotion.Infrastructure.Persistence;

Console.WriteLine("==============================================");
Console.WriteLine("  QuickCart Database Migrator (.NET 10)       ");
Console.WriteLine("==============================================");

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.SetBasePath(AppContext.BaseDirectory);
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        var connectionString = context.Configuration.GetConnectionString("QuickCartDb")
            ?? throw new InvalidOperationException("Connection string 'QuickCartDb' not configured.");

        services.AddDbContext<CatalogDbContext>(o => o.UseNpgsql(connectionString));
        services.AddDbContext<InventoryDbContext>(o => o.UseNpgsql(connectionString));
        services.AddDbContext<OrderingDbContext>(o => o.UseNpgsql(connectionString));
        services.AddDbContext<CartDbContext>(o => o.UseNpgsql(connectionString));
        services.AddDbContext<DeliveryDbContext>(o => o.UseNpgsql(connectionString));
        services.AddDbContext<IdentityDbContext>(o => o.UseNpgsql(connectionString));
        services.AddDbContext<PaymentDbContext>(o => o.UseNpgsql(connectionString));
        services.AddDbContext<PromotionDbContext>(o => o.UseNpgsql(connectionString));
    });

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    var contexts = new (string Name, DbContext Context)[]
    {
        ("Catalog", sp.GetRequiredService<CatalogDbContext>()),
        ("Inventory", sp.GetRequiredService<InventoryDbContext>()),
        ("Ordering", sp.GetRequiredService<OrderingDbContext>()),
        ("Cart", sp.GetRequiredService<CartDbContext>()),
        ("Delivery", sp.GetRequiredService<DeliveryDbContext>()),
        ("Identity", sp.GetRequiredService<IdentityDbContext>()),
        ("Payment", sp.GetRequiredService<PaymentDbContext>()),
        ("Promotion", sp.GetRequiredService<PromotionDbContext>())
    };

    foreach (var (name, context) in contexts)
    {
        Console.WriteLine($"[Migrating] Module: {name} (Schema: {context.Model.GetDefaultSchema()})...");
        await context.Database.MigrateAsync();
        Console.WriteLine($"✔ {name} migrations applied successfully.");
    }
}

Console.WriteLine("\nAll 8 module database migrations completed successfully!");
return 0;

