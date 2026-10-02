# Caching Architecture & Strategies

This document evaluates the caching architecture, memory caching abstractions, distributed caches, and cache invalidation policies across the QuickCart backend.

---

## 1. Architectural Status

```text
Status: Not Implemented / Not Confirmed

Evidence:
1. Inspected all 46 project files (.csproj) across src/, tests/, tools/, and benchmarks/. No caching packages (such as Microsoft.Extensions.Caching.Memory, Microsoft.Extensions.Caching.StackExchangeRedis, or EasyCaching) are referenced.
2. Inspected src/QuickCart.Api/Bootstrap/DependencyInjection.cs and MiddlewarePipeline.cs. Neither services.AddMemoryCache(), services.AddDistributedMemoryCache(), nor ASP.NET Core Output Caching (app.UseOutputCache()) are registered.
3. Inspected all C# source files under src/ (BuildingBlocks and Modules). No instances of IMemoryCache, IDistributedCache, or custom cache wrappers exist in backend code.
```

---

## 2. Current Request Pipeline

All HTTP requests dispatched to the backend query PostgreSQL directly via EF Core 10:
* **Catalog Queries**: Executed directly against `catalog.Products` and `catalog.Categories`.
* **Dark Store Queries**: Computed in-memory from `inventory.DarkStores` per request using `GeoLocationService`.
* **Cart & Order Lookups**: Fetched directly from `cart.Carts` and `ordering.Orders`.

> [!NOTE]
> While the backend does not implement caching, the Angular frontend (`quick-cart-app`) implements client-side fallback data caches in `CatalogService` to ensure uninterrupted UI rendering during transient backend restarts.

---

## 3. Future Architectural Roadmap (When Caching is Implemented)

For high-throughput quick-commerce scale (10-minute instant delivery), the following two-tier caching strategy should be evaluated:
1. **Tier 1 — In-Memory Cache (L1)**:
   * Cache frequently read, rarely changing taxonomy (e.g., categories, subcategories, brands) using `IMemoryCache`.
2. **Tier 2 — Distributed Redis Cache (L2)**:
   * Cache dark store inventory balances (`inventory.StoreInventories`) and user active cart states (`cart.Carts`) using `StackExchange.Redis`.
   * Enforce atomic decrements (`INCRBY` / Lua scripts) for real-time stock reservation to eliminate overselling during high-concurrency checkouts.
3. **Invalidation**: Publish domain events (`ProductPriceChanged`, `StockMovementRecorded`) to invalidate affected cache keys.

