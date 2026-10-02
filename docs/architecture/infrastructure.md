# Infrastructure Layer Architecture

This document details the persistence implementation, Entity Framework Core 10 mappings, repository patterns, interceptors, and external service adapters across the QuickCart infrastructure layer.

---

## 1. Role & Architectural Boundaries

The infrastructure layer (`<Module>.Infrastructure`) supplies concrete implementations for contracts defined in `<Module>.Application`:
* **Persistence**: Manages EF Core 10 `DbContext` models, schema mappings, database migrations, and repositories.
* **External Adapters**: Communicates with external identity providers (Google OAuth), payment gateways (HMAC cryptographic operations), and geolocation calculations.
* **Dependency Direction**: Implements application interfaces; never referenced by Domain or Application.

---

## 2. Shared Persistence Foundations (`BuildingBlocks.Infrastructure`)

### 2.1 Auditable Entity SaveChanges Interceptor
[`AuditableEntitySaveChangesInterceptor.cs`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/BuildingBlocks/BuildingBlocks.Infrastructure/Persistence/Interceptors/AuditableEntitySaveChangesInterceptor.cs) intercepts EF Core persistence commands:
```csharp
public sealed class AuditableEntitySaveChangesInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateAuditEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void UpdateAuditEntities(DbContext? context)
    {
        if (context is null) return;
        var utcNow = DateTime.UtcNow;
        var entries = context.ChangeTracker
            .Entries<IAuditableEntity>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added) entry.Entity.CreatedAt = utcNow;
            if (entry.State == EntityState.Modified) entry.Entity.UpdatedAt = utcNow;
        }
    }
}
```
Registered as a singleton in `src/QuickCart.Api/Bootstrap/DependencyInjection.cs` and attached to all 8 module `DbContext` instances.

---

## 3. Module DbContext Implementations

Each domain module implements an autonomous `DbContext` using PostgreSQL via `Npgsql.EntityFrameworkCore.PostgreSQL`:

| Module | DbContext Class | Schema Name | Entity Mappings Configured |
| :--- | :--- | :--- | :--- |
| **Catalog** | `CatalogDbContext` | `catalog` | `Products`, `Categories`, `SubCategories`, `Brands` |
| **Inventory** | `InventoryDbContext` | `inventory` | `DarkStores`, `StoreInventories`, `StockMovements` |
| **Ordering** | `OrderingDbContext` | `ordering` | `Orders`, `OrderItems`, `OrderStatusHistories` |
| **Cart** | `CartDbContext` | `cart` | `Carts`, `CartItems` |
| **Delivery** | `DeliveryDbContext` | `delivery` | `DeliveryPartners`, `DeliveryAssignments`, `DeliveryTrackings` |
| **Identity** | `IdentityDbContext` | `identity` | `Users`, `RefreshTokens`, `ExternalLogins` |
| **Payment** | `PaymentDbContext` | `payment` | `Payments`, `PaymentAuditLogs`, `PaymentWebhookEvents`, `Wallets` |
| **Promotion** | `PromotionDbContext` | `promotion` | `Coupons`, `Offers` |

### Fluent Schema Scoping
Every module `DbContext` explicitly registers its isolated schema:
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.HasDefaultSchema("<schema_name>");
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(<Module>DbContext).Assembly);
    base.OnModelCreating(modelBuilder);
}
```

---

## 4. Repositories & External Adapters

### 4.1 Concrete Repositories
* **`UserRepository`** (`Identity.Infrastructure`): User lookups by ID, email, phone, and refresh token hash (`GetByRefreshTokenHashAsync`).
* **`PaymentRepository`** (`Payment.Infrastructure`): Payment lookups by ID, order ID, provider order ID, and idempotency key.

### 4.2 External Service Adapters
* **`PaymentGatewayService`**: Implements `IPaymentGatewayService`. Manages gateway order creation, HMAC-SHA256 signature verification, constant-time equality checks, and refund simulation.
* **`GoogleTokenValidator`**: Implements `IGoogleTokenValidator`. Uses `Google.Apis.Auth` (`GoogleJsonWebSignature`) to validate Google OpenID Connect ID tokens against Google JWKS endpoints.
* **`JwtTokenService`**: Implements `IJwtTokenService`. Issues HMAC-SHA256 signed access tokens with custom identity claims and generates 64-byte CSPRNG refresh tokens.
* **`GeoLocationService`**: Implements spatial distance calculations (Haversine formula) to match customer coordinates against dark store catchment zones.

