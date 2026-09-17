# QuickCart — 10-Minute Quick Commerce Platform

`QuickCart.sln` is the enterprise-grade backend and application suite for an instant 10-minute grocery delivery platform. The repository combines a **.NET 10 Modular Monolith** backend API, shared architectural building blocks, 8 domain-isolated business modules, and a reactive **Angular 20** Single Page Application (SPA).

---

### 1. Project Overview

QuickCart is designed for ultra-fast grocery delivery, providing sub-minute order fulfillment, hyperlocal dark store inventory tracking, and real-time delivery telemetry.

* **Core Architecture**: **Modular Monolith** following **Clean Architecture (Domain-Driven Design / Onion Architecture)**. Every domain capability is encapsulated in an independent module with inward dependency rules (`Domain` $\leftarrow$ `Application` $\leftarrow$ `Infrastructure` & `Presentation`).
* **API Host**: ASP.NET Core 10 Web API host (`QuickCart.Api`) registering controllers, DbContexts, and middleware pipelines.
* **Technology Stack**:
  * **Framework**: .NET 10 (C# 14.0)
  * **Database & ORM**: PostgreSQL via Entity Framework Core 10 (`Npgsql`) with per-module schema boundaries (`catalog`, `inventory`, `ordering`, `cart`, `delivery`, `identity`, `payment`, `promotion`).
  * **Real-Time Communication**: SignalR WebSockets for live rider location updates and order status broadcasting.
  * **Frontend**: Angular 20 SPA using Angular Signals (`signal`, `computed`), standalone components, Geolocation API, and slide-out cart drawer.

---

### 2. High-Level Folder Structure

```text
Quick-commerce/
├── .config/               # Tooling configuration (dotnet-tools.json for dotnet-ef)
├── benchmarks/            # Performance benchmarking suites (QuickCart.BenchmarkHost)
├── docs/                  # Architecture specs, API contracts, database docs, ADRs
│   └── architecture/      # Architectural Decision Records (ADRs)
├── quick-cart-app/        # Modern Angular 20 reactive Single Page Application
├── scripts/               # PowerShell build, dev runner & database migration scripts
├── src/                   # Main application source code
│   ├── BuildingBlocks/    # Shared primitives across modules (Domain, App, Infra, Presentation)
│   ├── QuickCart.Api/     # Composition root & host for controllers and DbContexts
│   └── Modules/           # 8 Isolated domain modules (Clean Architecture)
├── tests/                 # Unit, integration, and module test suites
└── tools/                 # Migration runner CLI utilities (QuickCart.DatabaseMigrator)
```

---

### 3. Detailed Folder Breakdown

#### 📂 [src/](src/) — Application Source Code

Contains the core backend divided into three tiers:

##### 1. [src/QuickCart.Api/](src/QuickCart.Api/) (Composition Root)
The single entry point and web host for the backend.
* **[Program.cs](src/QuickCart.Api/Program.cs)**: Configures dependency injection, registers all 8 module DbContexts to PostgreSQL, and configures the ASP.NET Core middleware pipeline.
* **[appsettings.json](src/QuickCart.Api/appsettings.json)**: Runtime database connection string and secret configuration templates.

##### 2. [src/BuildingBlocks/](src/BuildingBlocks/) (Shared Primitives)
Cross-cutting architectural foundations shared across modules without containing business logic:
* **[BuildingBlocks.Domain](src/BuildingBlocks/BuildingBlocks.Domain/)**: Base entities, domain events, and value object primitives.
* **[BuildingBlocks.Application](src/BuildingBlocks/BuildingBlocks.Application/)**: CQRS abstractions, pagination metadata, and clock providers.
* **[BuildingBlocks.Infrastructure](src/BuildingBlocks/BuildingBlocks.Infrastructure/)**: EF Core base configurations, interceptors, and notification services.
* **[BuildingBlocks.Presentation](src/BuildingBlocks/BuildingBlocks.Presentation/)**: Shared API response formatting and global exception handling.

##### 3. [src/Modules/](src/Modules/) (Domain Modules)
Each module encapsulates a distinct quick commerce capability and follows strict inward **Clean Architecture** layers:
```text
<Module>/
├── <Module>.Domain          # Aggregates, entities, domain rules & invariants
├── <Module>.Application     # Use cases, commands, queries, service contracts
├── <Module>.Infrastructure  # EF Core DbContext, persistence mappings, migrations
└── <Module>.Presentation    # Controllers, endpoints, DTOs, SignalR hubs
```

Key Modules:
* **[Catalog](src/Modules/Catalog/)**: Master product definitions, categories, subcategories, brands, SKUs, and units of measure.
* **[Inventory](src/Modules/Inventory/)**: Dark stores registry, store-level inventory balances, stock movements, and geolocation catchment matching.
* **[Ordering](src/Modules/Ordering/)**: Order placement, price snapshots, line items, and lifecycle state transitions.
* **[Cart](src/Modules/Cart/)**: Shopping cart persistence, line item quantity updates, and cart subtotal derivation.
* **[Delivery](src/Modules/Delivery/)**: Delivery partner fleet, route ETA calculations, and SignalR live location hubs.
* **[Identity](src/Modules/Identity/)**: User accounts, phone/email verification, authentication, and customer delivery address book.
* **[Payment](src/Modules/Payment/)**: Payment transaction logging, wallet ledgers, and gateway integrations.
* **[Promotion](src/Modules/Promotion/)**: Discount coupons, promotional banners, and campaign rules.

---

#### 📂 [quick-cart-app/](quick-cart-app/) — Angular 20 Reactive Frontend

A client single-page application built on Angular 20 and TypeScript 5.9.
* **[core/](quick-cart-app/src/app/core/)**: API service, Auth interceptors/guards, Geolocation service, and SignalR client.
* **[layout/](quick-cart-app/src/app/layout/)**: Main layout, header with dynamic location selector, footer, and bottom navigation.
* **[modules/](quick-cart-app/src/app/modules/)**:
  * **`catalog/`**: Product listing page, category pill filters, and product cards.
  * **`cart/`**: Slide-out cart drawer with live subtotal/delivery fee calculation.
  * **`orders/`**: Checkout page with payment method selector and order confirmation page with 10-minute countdown tracking.
  * **`identity/`**: Login and registration components.

---

#### 📂 [docs/](docs/) — Architecture & Specifications

* **[ARCHITECTURE.md](docs/ARCHITECTURE.md)**: Deep dive into the modular monolith boundaries, clean architecture layers, and hyperlocal serviceability.
* **[DATABASE.md](docs/DATABASE.md)**: Database schemas, table definitions, entity relationships, and EF Core migration commands.
* **[API_CONTRACTS.md](docs/API_CONTRACTS.md)**: REST endpoints, request/response JSON schemas, and SignalR tracking events.
* **[DEVELOPMENT.md](docs/DEVELOPMENT.md)**: Developer machine setup, prerequisite installation, and local runtime instructions.
* **[architecture/](docs/architecture/)**: Architectural Decision Records (ADRs) covering dark store serviceability and order state transitions.

---

#### 📂 [tools/](tools/), [benchmarks/](benchmarks/) & [scripts/](scripts/)

* **[tools/QuickCart.DatabaseMigrator](tools/QuickCart.DatabaseMigrator/)**: Dedicated console tool for running EF Core migrations across all 8 module schemas.
* **[benchmarks/QuickCart.BenchmarkHost](benchmarks/QuickCart.BenchmarkHost/)**: BenchmarkDotNet harness for hot-path allocation and throughput measurements.
* **[scripts/build.ps1](scripts/build.ps1)**: PowerShell automation script to restore and build `QuickCart.sln`.
* **[scripts/migrate-database.ps1](scripts/migrate-database.ps1)**: Automation script to apply migrations for all or selected modules.
* **[scripts/run-dev.ps1](scripts/run-dev.ps1)**: One-click local startup script launching backend and frontend concurrently.

---

#### 📂 [tests/](tests/) — Automated Test Suites

* **[Catalog.Tests](tests/Catalog.Tests/)**: Unit tests for product catalog and pricing logic.
* **[Delivery.Tests](tests/Delivery.Tests/)**: Unit tests for rider assignment and ETA calculation formulas.
* **[Inventory.Tests](tests/Inventory.Tests/)**: Unit tests for stock reservation and dark store catchment matching.
* **[Ordering.Tests](tests/Ordering.Tests/)**: Unit tests for order totals and state machine transitions.
* **[QuickCart.IntegrationTests](tests/QuickCart.IntegrationTests/)**: End-to-end API integration tests using `WebApplicationFactory`.

