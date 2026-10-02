# QuickCart Backend — AI Agent Guidelines & Operating Rules

This document defines the authoritative operating rules, architectural boundaries, coding conventions, and engineering constraints for AI coding agents (and human engineers) working in the **QuickCart** repository.

---

## 1. Project Overview & Philosophy

* **Domain**: Quick Commerce (Instant Grocery Delivery) designed for 10-minute order fulfillment from localized dark stores (micro-fulfillment centers).
* **Architecture Style**: **Modular Monolith** built on **.NET 10 (C# 14)** using **Clean Architecture** (Domain-Driven Design / Onion Architecture) and **Multi-Schema PostgreSQL Isolation**.
* **Composition Host**: `src/QuickCart.Api` (ASP.NET Core Web API host).
* **Shared Foundations**: `src/BuildingBlocks` (Domain, Application, Infrastructure, Presentation primitives).
* **Domain Modules**: 8 autonomous modules under `src/Modules/`:
  1. `Catalog`
  2. `Inventory`
  3. `Ordering`
  4. `Cart`
  5. `Delivery`
  6. `Identity`
  7. `Payment`
  8. `Promotion`

---

## 2. Prime Operating Directives for AI Agents

1. **INSPECT BEFORE MODIFY**:
   * Always audit existing code, tests, configuration, and documentation before proposing or making changes.
   * Never assume a library, pattern, or table exists without verifying its presence in `.csproj`, `DbContext`, or source files.
2. **REUSE BEFORE CREATE**:
   * Inspect `BuildingBlocks` (`Result<T>`, `Error`, `Entity<T>`, `IAuditableEntity`, `NotFoundException`, `ValidationException`, `ConflictException`) before creating new primitives.
   * Reuse established repository, service, and DTO conventions from existing modules (e.g., `Identity` and `Payment`).
3. **DO NOT INVENT ARCHITECTURAL CLAIMS**:
   * If a capability or pattern is not implemented (e.g., caching, background workers, multi-tenancy, message brokers), document it as `Status: Not Implemented / Not Confirmed` with evidence. Never hallucinate implemented features.
4. **NEVER PERFORM DESTRUCTIVE GIT ACTIONS**:
   * Prohibited: `git reset`, `git restore`, `git checkout .`, `git clean`, `git stash`, `git rebase`, `git commit`, `git push`.
   * Preserve all existing uncommitted user changes at all times.
5. **DO NOT MODIFY FRONTEND FILES UNLESS EXPLICITLY INSTRUCTED**:
   * Files in `quick-cart-app/` are managed under frontend workflows. Do not alter frontend files during backend tasks.
6. **PRESERVE EXISTING DOCUMENTATION INTEGRITY**:
   * Retain existing comments, docstrings, and verified documentation unless specifically instructed to update them.

---

## 3. Technology Stack Baseline

| Component | Verified Technology | Notes |
| :--- | :--- | :--- |
| **Runtime** | .NET 10 (`net10.0`), C# 14.0 | Enforced via `Directory.Build.props` and `global.json` (`10.0.401`) |
| **Web Host** | ASP.NET Core 10 Web API | Minimal API bootstrap + Controller mapping (`QuickCart.Api`) |
| **ORM / Data Access** | Entity Framework Core 10 (`10.0.4`) | PostgreSQL provider: `Npgsql.EntityFrameworkCore.PostgreSQL` (`10.0.3`) |
| **Database** | PostgreSQL 16+ | Multi-schema architecture (`catalog`, `inventory`, `ordering`, etc.) |
| **Real-Time** | ASP.NET Core SignalR | WebSocket tracking hub at `/hubs/delivery-tracking` |
| **API Docs** | Microsoft.AspNetCore.OpenApi (`10.0.12`) | Scalar interactive documentation (`Scalar.AspNetCore` `2.17.3`) |
| **Auth & Security** | JWT Bearer (`10.0.12`), BCrypt.Net-Next (`4.2.0`) | Google OpenID Connect token validation (`Google.Apis.Auth` `1.76.0`) |
| **Testing** | xUnit (`2.9.3`), Moq (`4.20.72`), Coverlet (`6.0.4`) | Microsoft.NET.Test.Sdk (`17.14.1`) |
| **Benchmarking** | BenchmarkDotNet (`0.14.0`) | Located in `benchmarks/QuickCart.BenchmarkHost` |

---

## 4. Architecture & Layering Rules

Every domain module (`src/Modules/<Module>/`) is divided into 4 Clean Architecture projects:

```text
[ Domain ] <── [ Application ] <── [ Infrastructure ]
                               <── [ Presentation ]
```

### Inward Dependency Rules
1. **`<Module>.Domain`**:
   * Contains aggregates, entities, value objects, domain events, domain errors, and enums.
   * **Zero external dependencies**: Depends only on `BuildingBlocks.Domain`. Never reference EF Core, ASP.NET Core, or Infrastructure.
2. **`<Module>.Application`**:
   * Contains use cases, DTOs, service interfaces, command/query contracts, and validation logic.
   * Depends only on `<Module>.Domain` and `BuildingBlocks.Application`.
   * Never reference `Microsoft.EntityFrameworkCore` or data access infrastructure.
3. **`<Module>.Infrastructure`**:
   * Contains `DbContext`, EF Core entity configurations (`IEntityTypeConfiguration<T>`), migrations, repository implementations, and external service clients.
   * Implements interfaces defined in `<Module>.Application`.
   * Depends on `<Module>.Application`, `<Module>.Domain`, and `BuildingBlocks.Infrastructure`.
4. **`<Module>.Presentation`**:
   * Contains API controllers (`[ApiController]`), route models, and SignalR hubs.
   * Depends on `<Module>.Application`, `<Module>.Domain`, and `BuildingBlocks.Presentation`.
   * Controllers never access `DbContext` directly; they call application services/handlers.

### Module Isolation Rules
* **No Cross-Schema Foreign Keys**: Foreign keys across different schemas are strictly prohibited.
* **Cross-Module References by Identity**: Reference entities in other modules exclusively by immutable `Guid` identifiers (e.g., `UserId`, `OrderId`, `ProductId`, `DeliveryAddressId`).
* **Inter-Module Communication**:
  * In-process: A module may reference another module's `<OtherModule>.Application` interface (e.g., `Payment` references `Ordering.Application.Services.IOrderService`).
  * Cross-module direct DbContext access is strictly forbidden.

---

## 5. Coding & API Conventions

* **C# Language Features**: C# 14.0; nullable reference types are enabled (`<Nullable>enable</Nullable>`). Every parameter, return type, and property must account for nullability.
* **Timestamps**: All timestamps must be in UTC (`DateTime.UtcNow`). Handled automatically for auditable entities via `AuditableEntitySaveChangesInterceptor`.
* **Async/Await**:
  * Every I/O operation (database, network, file) must be asynchronous (`async`/`await`).
  * Accept and pass `CancellationToken` through all asynchronous call chains.
* **API Endpoints**:
  * Controllers inherit `ControllerBase` and use attribute routing: `[Route("api/[controller]")]` or explicit paths like `[Route("api/auth")]`.
  * Return standard HTTP status codes: `200 OK`, `201 Created`, `204 No Content`, `400 Bad Request`, `401 Unauthorized`, `404 Not Found`, `409 Conflict`.
* **Error Handling**:
  * Do not catch exceptions unless you can meaningfully handle or enrich them.
  * Throw domain/application exceptions: `NotFoundException`, `ValidationException`, `ConflictException`, `UnauthorizedAccessException`.
  * `GlobalExceptionHandler` converts these automatically into standard RFC 7807 Problem Details (`application/problem+json`).

---

## 6. Database & Migration Rules

* **Default Schema per DbContext**: Each module DbContext must set its schema in `OnModelCreating`:
  ```csharp
  modelBuilder.HasDefaultSchema("<module_name>");
  ```
* **Migration Isolation**: Each module maintains its own migration stream under `src/Modules/<Module>/<Module>.Infrastructure/Persistence/Migrations/`.
* **Adding Migrations**:
  ```powershell
  dotnet ef migrations add <MigrationName> `
      --project src/Modules/<ModuleName>/<ModuleName>.Infrastructure/<ModuleName>.Infrastructure.csproj `
      --startup-project src/QuickCart.Api/QuickCart.Api.csproj `
      --context <ModuleName>DbContext `
      --output-dir Persistence/Migrations
  ```
* **Applying Migrations**:
  * Use `./scripts/migrate-database.ps1` or `dotnet run --project tools/QuickCart.DatabaseMigrator`.
* **Rule**: Never edit existing migration files or snapshots manually. Never modify database schemas outside of EF Core migrations.

---

## 7. Security Rules

* **Server-Verified Totals**: Never trust client-supplied amounts or prices. Prices and totals must be recalculated or verified server-side from authoritative database records (see `PaymentService.CreatePaymentIntentAsync`).
* **IDOR Protection**: Always verify that the authenticated caller (`userId` from JWT claims) matches the resource owner before performing operations on orders, carts, addresses, or payments.
* **Password Hashing**: Use BCrypt via `BCrypt.Net-Next` (`EnhancedHashPassword` / `EnhancedVerify`). Never store or log plain text passwords.
* **Refresh Tokens**: Store only cryptographic SHA-256 hashes of refresh tokens in `identity.RefreshTokens`. Enforce rotation on refresh and revocation on logout.
* **Payment Webhooks**:
  * Validate webhook signatures using constant-time HMAC-SHA256 (`CryptographicOperations.FixedTimeEquals`).
  * Enforce idempotency via `payment.PaymentWebhookEvents` deduplication on `ProviderEventId`.
* **Secrets**: Never commit secrets, API keys, or JWT signing keys. Store them in user secrets or environment variables in production.

---

## 8. Testing Conventions

* **Framework**: xUnit with Moq.
* **Test Structure**: Arrange, Act, Assert (AAA).
* **Test Isolation**: Unit tests must not require an active database or network connection; mock interfaces (`IOrderService`, `IUserRepository`, `IPaymentGatewayService`).
* **Verification**: Run `dotnet test QuickCart.sln` to verify tests pass before reporting completion.

---

## 9. AI Workflow Summary

When assigned a task:
1. **Audit**: Read relevant files, check project references, review tests and documentation.
2. **Plan**: Formulate the minimal, safe change set respecting all architectural boundaries.
3. **Implement**: Write clean, idiomatic, fully-typed C# code without breaking existing functionality.
4. **Test**: Run `dotnet build QuickCart.sln` and `dotnet test QuickCart.sln`.
5. **Report**: Summarize verified changes, test results, and any unconfirmed topics.

