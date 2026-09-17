namespace QuickCart.Api.Bootstrap;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Delivery.Presentation.Hubs;

public static class EndpointRegistration
{
    public static IEndpointRouteBuilder MapApplicationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // 1. Health Probe
        endpoints.MapGet("/health", () => Results.Ok(new
        {
            Status = "Healthy",
            Service = "QuickCart.Api",
            Timestamp = DateTime.UtcNow
        }))
        .WithName("HealthCheck")
        .WithTags("Health");

        // 2. Controllers across all modules
        endpoints.MapControllers();

        // 3. Real-Time SignalR Hub
        endpoints.MapHub<DeliveryTrackingHub>("/hubs/delivery-tracking");

        return endpoints;
    }
}

