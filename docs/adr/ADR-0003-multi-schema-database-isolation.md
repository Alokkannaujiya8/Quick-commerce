# ADR-0003: Multi-Schema PostgreSQL Isolation without Cross-Schema Keys

## Status
Approved

## Context
A modular monolith requires strict boundary enforcement between domain modules. Allowing cross-module table joins and physical database foreign keys across module boundaries creates tight coupling, rendering future service extraction impossible and creating cross-domain migration deadlocks.

## Decision
1. **Multi-Schema Model**: Each business module exclusively owns an isolated PostgreSQL schema (`catalog`, `inventory`, `ordering`, `cart`, `delivery`, `identity`, `payment`, `promotion`) inside `quickcartdb`.
2. **Dedicated DbContexts**: Each module defines its own EF Core `DbContext` scoped to its schema via `modelBuilder.HasDefaultSchema("<schema>")`.
3. **No Cross-Schema Foreign Keys**: Foreign keys across different schemas are strictly forbidden. Cross-module entity references are stored exclusively as immutable `Guid` values.
4. **Independent Migration Streams**: Each module maintains its own migration stream under `src/Modules/<Module>/<Module>.Infrastructure/Persistence/Migrations/`.

## Consequences
### Positive
* Enforces domain autonomy and prevents accidental monolithic coupling.
* Enables any high-load module (e.g. `Delivery` or `Inventory`) to be extracted into a standalone microservice with zero schema refactoring.
* Modules can evolve their data models independently.

### Negative / Trade-offs
* Requires multiple DbContext registrations and separate migration commands.
* Data consistency across modules cannot rely on database cascade deletes and must be managed at the application/domain level.

## Verification & Code References
* DbContexts: All 8 `*DbContext.cs` files under `src/Modules/*/Infrastructure/Persistence/`
* Configuration: [`DependencyInjection.cs`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/QuickCart.Api/Bootstrap/DependencyInjection.cs)
* Scripts: [`scripts/migrate-database.ps1`](file:///d:/Asp.net%20core_Project/Quick-commerce/scripts/migrate-database.ps1)

