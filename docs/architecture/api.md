# API Layer & Presentation Architecture

This document details the HTTP host design, endpoint mappings, middleware pipeline, CORS policies, interactive documentation, and SignalR hub endpoints of `QuickCart.Api`.

---

## 1. Composition Root Overview

`src/QuickCart.Api` functions as the sole composition root for the modular monolith. It binds together the domain modules, configures cross-cutting infrastructure (PostgreSQL DbContexts, JWT authentication, exception filters), and defines the HTTP pipeline.

### Entry Point (`Program.cs`)
```csharp
var builder = WebApplication.CreateBuilder(args);

// Modular Architecture Bootstrap
builder.Services.AddApiHost(builder.Configuration, builder.Environment);

var app = builder.Build();

// Automatically apply pending EF Core migrations on startup in Development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var identityDbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    await identityDbContext.Database.MigrateAsync();
}

// Middleware Pipeline & Endpoints
app.UseApplicationPipeline();
app.MapApplicationEndpoints();

await app.RunAsync();
```

---

## 2. HTTP Middleware Pipeline

Configured in [`MiddlewarePipeline.cs`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/QuickCart.Api/Bootstrap/MiddlewarePipeline.cs), the execution order is strictly orchestrated:

1. **`app.UseExceptionHandler()`**: Intercepts all downstream exceptions, delegating them to `GlobalExceptionHandler` to produce RFC 7807 Problem Details.
2. **OpenAPI & Scalar Documentation**:
   * `app.MapOpenApi()`: Generates OpenAPI v3 specifications.
   * `app.MapScalarApiReference()`: Serves interactive documentation with `ScalarTheme.Purple` at `/scalar/v1` in Development mode.
3. **`app.UseHttpsRedirection()`**: Redirects HTTP requests to secure HTTPS.
4. **`app.UseRouting()`**: Matches incoming HTTP requests to endpoints.
5. **`app.UseCors(FrontendCorsPolicy)`**: Applies Cross-Origin Resource Sharing rules.
6. **`app.UseAuthentication()`**: Validates JWT bearer tokens and constructs `ClaimsPrincipal`.
7. **`app.UseAuthorization()`**: Enforces role and policy authorization attributes (`[Authorize]`).

---

## 3. CORS Policy Configuration

Configured in [`DependencyInjection.cs`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/QuickCart.Api/Bootstrap/DependencyInjection.cs):
* **Policy Name**: `QuickCartFrontendPolicy`
* **Allowed Origins**: `http://localhost:4200`, `http://localhost:3000`, `https://localhost:4200`
* **Headers & Methods**: `AllowAnyHeader()`, `AllowAnyMethod()`
* **Credentials**: `AllowCredentials()` enabled for cookie and Authorization header transmission.

---

## 4. Endpoint Routing & SignalR Mapping

Configured in [`EndpointRegistration.cs`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/QuickCart.Api/Bootstrap/EndpointRegistration.cs):

### 4.1 Built-in Health Endpoint
* **Route**: `GET /health`
* **Response**: `200 OK`
  ```json
  {
    "status": "Healthy",
    "service": "QuickCart.Api",
    "timestamp": "2026-10-02T06:20:00Z"
  }
  ```

### 4.2 Controller Endpoints
`endpoints.MapControllers()` scans all referenced `<Module>.Presentation` assemblies for classes inheriting `ControllerBase` annotated with `[ApiController]`:
* `AuthController` (`/api/auth/*`)
* `ProductsController` (`/api/catalog/*`, `/api/products/*`, `/api/categories`)
* `CartController` (`/api/cart/*`)
* `OrdersController` (`/api/orders/*`)
* `PaymentsController` (`/api/payments/*`)
* `DarkStoresController` (`/api/dark-stores`, `/api/serviceability`)
* `DeliveryController` (`/api/orders/{orderId}/assign`, `/api/orders/{orderId}/tracking`)

### 4.3 SignalR Telemetry Hub
* **Hub Type**: `DeliveryTrackingHub`
* **Route**: `/hubs/delivery-tracking`
* **Transports**: WebSockets with fallback to Long Polling.

