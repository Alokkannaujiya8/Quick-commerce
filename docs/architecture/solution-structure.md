# Solution Structure & Project Organization

This document details the physical repository organization, project hierarchy, MSBuild configuration, and assembly boundary layout of `QuickCart.sln`.

---

## 1. High-Level Repository Layout

```text
Quick-commerce/
├── .config/                    # Tooling manifests (.NET Local Tools: dotnet-ef)
├── benchmarks/                 # Benchmarking suites (BenchmarkDotNet)
│   └── QuickCart.BenchmarkHost/
├── docs/                       # Architecture, engineering rules, ADRs, AI memory
├── quick-cart-app/             # Angular 20 Single Page Application
├── scripts/                    # PowerShell automation scripts (build, migrate, run)
├── src/                        # Core backend source projects
│   ├── BuildingBlocks/         # Shared architectural foundation
│   ├── Modules/                # 8 Domain-isolated business modules
│   └── QuickCart.Api/          # Composition root & ASP.NET Core host
├── tests/                      # Unit, integration, and module test suites
├── tools/                      # CLI utilities (DatabaseMigrator)
├── Directory.Build.props       # Solution-wide MSBuild compiler properties
├── global.json                 # .NET SDK pin (10.0.401)
├── QuickCart.sln               # Primary solution file (46 projects)
└── README.md                   # Repository landing page
```

---

## 2. MSBuild Solution-Wide Settings

Configured in [`Directory.Build.props`](file:///d:/Asp.net%20core_Project/Quick-commerce/Directory.Build.props):
```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <Deterministic>true</Deterministic>
    <LangVersion>14.0</LangVersion>
  </PropertyGroup>
</Project>
```
* **Target Framework**: All projects compile against `.NET 10.0`.
* **C# Version**: Strictly C# 14.0.
* **Nullable Reference Types**: Mandatory null-safety analysis active across all 46 projects.
* **SDK Enforcement**: [`global.json`](file:///d:/Asp.net%20core_Project/Quick-commerce/global.json) enforces SDK version `10.0.401` with `latestFeature` roll-forward.

---

## 3. Project Classification & Breakdown

`QuickCart.sln` aggregates 46 projects categorized into 6 distinct tiers:

### 3.1 Composition Root (`src/QuickCart.Api`)
* **Project**: `src/QuickCart.Api/QuickCart.Api.csproj`
* **Role**: Entry point for the backend runtime. Registers dependency injection containers, configures the HTTP request pipeline, hosts SignalR hubs, and serves the Scalar OpenAPI interactive documentation.

### 3.2 Shared Building Blocks (`src/BuildingBlocks`)
Cross-cutting architectural foundations shared across all modules without containing business logic:
* **`BuildingBlocks.Domain`**: Entity primitives (`Entity<TId>`, `AuditableEntity<TId>`), domain events (`IDomainEvent`), and audit contracts (`IAuditableEntity`).
* **`BuildingBlocks.Application`**: CQRS interfaces (`ICommand`, `IQuery`, `ICommandHandler`, `IQueryHandler`), Result pattern (`Result`, `Result<TValue>`, `Error`), and pagination structures (`PagedResult<T>`).
* **`BuildingBlocks.Infrastructure`**: EF Core interceptors (`AuditableEntitySaveChangesInterceptor`), shared persistence conventions, and base services.
* **`BuildingBlocks.Presentation`**: Global exception handler (`GlobalExceptionHandler`), RFC 7807 problem details serialization, and standard API responses (`ApiResponse<T>`).

### 3.3 Domain Modules (`src/Modules/`)
Each of the 8 business capabilities contains 4 Clean Architecture subprojects (32 projects total):
1. **Cart**: `Cart.Domain`, `Cart.Application`, `Cart.Infrastructure`, `Cart.Presentation`
2. **Catalog**: `Catalog.Domain`, `Catalog.Application`, `Catalog.Infrastructure`, `Catalog.Presentation`
3. **Delivery**: `Delivery.Domain`, `Delivery.Application`, `Delivery.Infrastructure`, `Delivery.Presentation`
4. **Identity**: `Identity.Domain`, `Identity.Application`, `Identity.Infrastructure`, `Identity.Presentation`
5. **Inventory**: `Inventory.Domain`, `Inventory.Application`, `Inventory.Infrastructure`, `Inventory.Presentation`
6. **Ordering**: `Ordering.Domain`, `Ordering.Application`, `Ordering.Infrastructure`, `Ordering.Presentation`
7. **Payment**: `Payment.Domain`, `Payment.Application`, `Payment.Infrastructure`, `Payment.Presentation`
8. **Promotion**: `Promotion.Domain`, `Promotion.Application`, `Promotion.Infrastructure`, `Promotion.Presentation`

### 3.4 Automated Test Projects (`tests/`)
* **`Identity.Tests`**: Unit, domain, and controller tests for authentication and user management (41 active tests).
* **`Payment.Tests`**: Unit tests for payment intents, webhook deduplication, signature verification, and refunds (14 active tests).
* **`Catalog.Tests`**, **`Delivery.Tests`**, **`Inventory.Tests`**, **`Ordering.Tests`**, **`QuickCart.IntegrationTests`**: Skeleton test project files established for future test expansion.

### 3.5 Benchmarks (`benchmarks/`)
* **`QuickCart.BenchmarkHost`**: Dedicated BenchmarkDotNet project for performance profiling and allocation measurement.

### 3.6 Tooling (`tools/`)
* **`QuickCart.DatabaseMigrator`**: Dedicated standalone CLI application for executing multi-schema EF Core migrations across PostgreSQL.

