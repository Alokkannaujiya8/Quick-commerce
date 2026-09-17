using Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using QuickCart.Api.Bootstrap;

var builder = WebApplication.CreateBuilder(args);

// Modular Architecture Bootstrap
builder.Services.AddApiHost(builder.Configuration, builder.Environment);

var app = builder.Build();

// Automatically apply pending EF Core migrations on startup
try
{
    using var scope = app.Services.CreateScope();
    var identityDbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    await identityDbContext.Database.MigrateAsync();
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "An error occurred while applying database migrations for IdentityDbContext.");
}

// Middleware Pipeline & Endpoints
app.UseApplicationPipeline();
app.MapApplicationEndpoints();

await app.RunAsync();

