# Performance & Optimization Practices

This document outlines the verified performance practices, EF Core query optimizations, asynchronous programming patterns, and benchmarking harnesses implemented in the QuickCart backend.

---

## 1. Verified Data Access Practices (EF Core 10)

### 1.1 Read-Only Query Tracking Disabling (`AsNoTracking`)
For read queries that do not mutate entities, tracking overhead is eliminated using `.AsNoTracking()`:
```csharp
// Example from OrderService.cs
var orders = await _context.Orders
    .AsNoTracking()
    .Where(o => o.UserId == userId)
    .OrderByDescending(o => o.PlacedAt)
    .Include(o => o.OrderItems)
    .Include(o => o.OrderStatusHistories)
    .ToListAsync(cancellationToken);
```
**Benefits**: Bypasses the EF Core Change Tracker snapshotting mechanism, significantly reducing allocations and query execution time.

### 1.2 High-Velocity Database Indexing
The database schema defines explicit indexes on frequently filtered, queried, and joined columns:
* **Unique Lookup Indexes**:
  * `catalog.Products`: `Slug`, `SKU`
  * `catalog.Categories`: `Slug`
  * `ordering.Orders`: `OrderNumber`
  * `identity.Users`: `Email`, `PhoneNumber`
  * `identity.RefreshTokens`: `TokenHash`
  * `identity.ExternalLogins`: `(Provider, ProviderUserId)`
  * `payment.Payments`: `ProviderOrderId`, `IdempotencyKey`
  * `payment.PaymentWebhookEvents`: `ProviderEventId`
* **Foreign & Partition Key Indexes**:
  * `ordering.Orders`: `UserId`
  * `payment.Payments`: `OrderId`
  * `identity.RefreshTokens`: `UserId`

### 1.3 Asynchronous Execution & Cancellation Propagation
All data access operations leverage non-blocking async APIs:
* `SaveChangesAsync(cancellationToken)`
* `FirstOrDefaultAsync(cancellationToken)`
* `ToListAsync(cancellationToken)`
* `AddAsync(entity, cancellationToken)`
All async service signatures accept `CancellationToken cancellationToken = default` to allow terminating queries if the HTTP client disconnects early.

---

## 2. Microbenchmarking Harness

* **Project**: [`benchmarks/QuickCart.BenchmarkHost`](file:///d:/Asp.net%20core_Project/Quick-commerce/benchmarks/QuickCart.BenchmarkHost/)
* **Engine**: `BenchmarkDotNet` (`0.14.0`)
* **Purpose**: Profiling latency, memory allocations, and CPU cycles on hot-path algorithms (such as geolocation distance calculations, cryptographic hashing, and order line total aggregations).

---

## 3. Caching Architecture

* **Status**: `Status: Not Implemented / Not Confirmed`
* **Evidence**:
  * Inspected all `.csproj` files, `QuickCart.Api/Program.cs`, and `DependencyInjection.cs`.
  * No caching libraries or middleware (`Microsoft.Extensions.Caching.Memory`, `Microsoft.Extensions.Caching.StackExchangeRedis`, `IDistributedCache`, or ASP.NET Core Output Caching) are currently configured in the backend.
  * Note: The Angular frontend implements client-side fallback caches, but server-side caching remains unconfigured.

---

## 4. Background Job Processing

* **Status**: `Status: Not Implemented / Not Confirmed`
* **Evidence**:
  * Inspected all module source trees for `IHostedService`, `BackgroundService`, `System.Threading.Channels`, Hangfire, or Quartz.
  * No background workers or queue processing daemons are currently present. All order and payment processing executes in-process during HTTP request lifecycles.

