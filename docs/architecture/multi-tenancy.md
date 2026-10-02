# Multi-Tenancy Architecture

This document evaluates the multi-tenancy model and isolation boundaries within the QuickCart backend repository.

---

## 1. Architectural Status

```text
Status: Not Implemented / Not Confirmed

Evidence:
1. Inspected all C# source files under src/ (BuildingBlocks, Modules, QuickCart.Api) for tenancy abstractions (e.g., ITenantProvider, ITenantContext, TenantId, MultiTenancyMiddleware). None exist.
2. Inspected database entities across all 8 modules (Catalog, Inventory, Ordering, Cart, Delivery, Identity, Payment, Promotion). No TenantId columns, composite tenant keys, or global EF Core query filters exist.
3. Inspected configuration files (appsettings.json, appsettings.Development.json). No multi-tenant connection string resolvers or tenant mapping sections are defined.
```

---

## 2. Current Architecture: Single-Tenant Model

The QuickCart platform currently operates as an enterprise **single-tenant deployment**:
* All registered users, dark stores, catalogs, and orders exist within a single unified operational namespace.
* Hyperlocal isolation is enforced by **geographic catchment zones** (`DarkStore.Latitude`, `DarkStore.Longitude`, `DarkStore.ServiceRadiusKm`) rather than organizational tenant boundaries.
* Data isolation is achieved at the **domain module schema level** (`catalog`, `inventory`, `ordering`, etc.) rather than by tenant partitions.

---

## 3. Future Architectural Considerations (If Multi-Tenancy is Requested)

If multi-tenancy (e.g., white-labeling the quick-commerce engine for multiple commercial brands) is required in the future:
1. Introduce `ITenantContext` and `TenantResolutionMiddleware` into `BuildingBlocks.Application` and `BuildingBlocks.Presentation`.
2. Implement tenant discriminators (`TenantId`) or database schema-per-tenant strategies.
3. Add global query filters via EF Core `OnModelCreating` to prevent cross-tenant data leakage.

