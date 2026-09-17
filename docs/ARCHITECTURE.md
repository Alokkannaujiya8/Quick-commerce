# QuickCart Architecture Specification

This document describes the architectural foundation, boundaries, patterns, and design decisions for the **QuickCart** Quick Commerce (Instant Grocery Delivery) platform.

---

## 1. System Purpose & Core Domain

QuickCart is an enterprise-grade, high-throughput **10-minute instant delivery** platform. Unlike standard e-commerce with multi-day fulfillment windows, QuickCart operates under extreme temporal and operational constraints:

1. **Hyperlocal Dark Store Inventory**: Customer orders are serviced exclusively from localized dark stores (micro-fulfillment centers) located within a 3–5 km radius.
2. **Sub-Minute Order Dispatch**: Fast order assembly, automatic rider assignment, and dispatch within 2–3 minutes of placement.
3. **Real-Time Delivery Tracking**: Live telemetry, route progress, and dynamic ETA estimation powered by WebSockets/SignalR.
4. **Resilient High-Concurrency Checkout**: Real-time stock reservation and deduction to prevent overselling high-velocity fast-moving consumer goods (FMCG).

---

## 2. Architectural Style: Modular Monolith

QuickCart is architected as a **Modular Monolith** using **.NET 10 (C# 14)** and **Clean Architecture (Domain-Driven Design / Onion Architecture)**.

### Why Modular Monolith?
- **Domain Autonomy**: Each business module maintains independent domain rules, data storage models, and business logic without premature network boundaries (microservice network overhead).
- **Independent Schema Ownership**: Each module owns its isolated PostgreSQL schema (`catalog`, `inventory`, `ordering`, `cart`, `delivery`, `identity`, `payment`, `promotion`). Cross-module joins are forbidden; integration occurs via strongly-typed application contracts and domain events.
- **Future Microservices Extraction**: If high-load modules (e.g., `Delivery` tracking or `Inventory` locking) require independent scaling, their clean boundaries allow extracting them into standalone microservices with zero architectural refactoring.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              QuickCart.Api                                  │
│                 (HTTP Minimal APIs / Controller Host)                       │
└───────┬──────────────┬──────────────┬──────────────┬──────────────┬─────────┘
        │              │              │              │              │
┌───────▼──────┐┌──────▼──────┐┌──────▼──────┐┌──────▼──────┐┌──────▼──────┐
│   Catalog    ││  Inventory  ││   Ordering   ││   Delivery   ││  Identity   │ ...
│  (catalog)   ││ (inventory) ││  (ordering)  ││  (delivery)  ││ (identity)  │
└───────┬──────┘└──────┬──────┘└──────┬──────┘└──────┬──────┘└──────┬──────┘
        └──────────────┴──────────────┼──────────────┴──────────────┘
                                      │
                   ┌──────────────────▼──────────────────┐
                   │           BuildingBlocks            │
                   │  (Domain, App, Infra, Presentation) │
                   └─────────────────────────────────────┘
```

---

## 3. Clean Architecture Layering (Per Module)

Every domain module adheres strictly to inward dependency inversion:

```
[ Domain ] <── [ Application ] <── [ Infrastructure ]
                               <── [ Presentation ]
```

### Layer Responsibilities

#### 1. Domain Layer (`<Module>.Domain`)
* **Role**: Pure business entities, value objects, domain events, and domain exceptions.
* **Dependencies**: None. Zero external NuGet packages or framework coupling.
* **Invariants**: All business validation rules and state invariants live here.

#### 2. Application Layer (`<Module>.Application`)
* **Role**: Use-case orchestration, CQRS commands, queries, validators, DTOs, and interface definitions.
* **Dependencies**: Depends strictly on `<Module>.Domain` and `BuildingBlocks.Application`.
* **Rules**: Never references EF Core, databases, or HTTP frameworks.

#### 3. Infrastructure Layer (`<Module>.Infrastructure`)
* **Role**: Concrete data access (EF Core `DbContext`), external API clients, messaging adapters, and file storage.
* **Dependencies**: Implements interfaces defined in `<Module>.Application`.
* **Persistence**: Owns its module-specific EF Core migration history and table configurations.

#### 4. Presentation Layer (`<Module>.Presentation`)
* **Role**: API controllers, endpoints, request validators, and SignalR hubs.
* **Dependencies**: Calls use cases defined in `<Module>.Application`.

---

## 4. Domain Module Boundaries

### 🛒 1. Cart (`Cart.*`)
* **Schema**: `cart`
* **Boundary**: Ephemeral user cart storage, item quantity increments/decrements, subtotal computation, and cart validation prior to checkout.
* **Key Aggregates**: `Cart`, `CartItem`.

### 🏷️ 2. Catalog (`Catalog.*`)
* **Schema**: `catalog`
* **Boundary**: Store-agnostic master product definitions, taxonomy (categories, subcategories), brands, SKUs, and unit-of-measure specifications.
* **Key Aggregates**: `Product`, `Category`, `SubCategory`, `Brand`.
* **Design Decision**: Products are uniquely identified by SKU and SEO-friendly slug.

### 🏬 3. Inventory (`Inventory.*`)
* **Schema**: `inventory`
* **Boundary**: Dark store definitions, store-to-product stock allocation (`StoreInventory`), and immutable stock audit trail (`StockMovement`).
* **Key Aggregates**: `DarkStore`, `StoreInventory`, `StockMovement`.
* **Serviceability**: Matches customer GPS coordinates against dark store service radii using `GeoLocationService`.

### 📦 4. Ordering (`Ordering.*`)
* **Schema**: `ordering`
* **Boundary**: Durable order placement, item price snapshots at order time, order status lifecycle management, and cancellation handling.
* **Key Aggregates**: `Order`, `OrderItem`, `OrderStatusHistory`.
* **State Machine**:
  $$\text{Placed} \longrightarrow \text{Confirmed} \longrightarrow \text{Packed} \longrightarrow \text{OutForDelivery} \longrightarrow \text{Delivered}$$
  $$\text{Placed} \longrightarrow \text{Cancelled} \quad\text{or}\quad \text{Delivered} \longrightarrow \text{Returned}$$

### 🛵 5. Delivery (`Delivery.*`)
* **Schema**: `delivery`
* **Boundary**: Delivery partner fleet, order assignment, real-time rider GPS tracking, and route ETA calculations.
* **Key Aggregates**: `DeliveryPartner`, `DeliveryAssignment`, `DeliveryTracking`.
* **Real-time Pipeline**: `DeliveryTrackingHub` provides WebSocket/SignalR streams to the customer frontend.

### 👤 6. Identity (`Identity.*`)
* **Schema**: `identity`
* **Boundary**: Customer and partner accounts, credentials, contact verification (email/phone), and saved delivery addresses with coordinates.
* **Key Aggregates**: `ApplicationUser`, `Address`.

### 💳 7. Payment (`Payment.*`)
* **Schema**: `payment`
* **Boundary**: Payment transaction tracking, gateway integrations (UPI, Cards, Cash on Delivery), and customer wallet balances.
* **Key Aggregates**: `Payment`, `Wallet`.

### 🎟️ 8. Promotion (`Promotion.*`)
* **Schema**: `promotion`
* **Boundary**: Discount rules, coupon validation, promotional banners, and campaign management.
* **Key Aggregates**: `Coupon`, `Offer`.

---

## 5. Shared Building Blocks (`src/BuildingBlocks`)

The `BuildingBlocks` projects supply foundational primitives to eliminate duplicate plumbing across modules:

* **`BuildingBlocks.Domain`**: Base entity classes (`Entity<TId>`, `AggregateRoot<TId>`), domain event interfaces (`IDomainEvent`), and value object base types.
* **`BuildingBlocks.Application`**: CQRS interfaces (`ICommand`, `IQuery`, `ICommandHandler`, `IQueryHandler`), paged query specifications, and clock abstractions.
* **`BuildingBlocks.Infrastructure`**: EF Core conventions, interceptors for automatic `CreatedAt`/`UpdatedAt` population, and notification dispatchers.
* **`BuildingBlocks.Presentation`**: Standard RFC 7807 Problem Details error responses, API versioning, and global exception filters.

---

## 6. Frontend Architecture (`quick-cart-app`)

The frontend is an Angular 20 Single Page Application designed for speed, responsiveness, and instant interaction:

* **Reactive State via Angular Signals**:
  `CartService` uses modern Angular Signals (`signal()`, `computed()`) to derive items, counts, subtotals, and delivery fee thresholds dynamically without RxJS boilerplate.
* **Location-First User Experience**:
  `GeolocationService` queries browser GPS to determine the user's nearest dark store automatically, personalizing product availability and estimated delivery times.
* **Real-Time Tracking Client**:
  `SignalRService` connects to `/hubs/delivery-tracking` to receive rider movement updates and order state changes.
* **Offline-Resilient Fallback**:
  `CatalogService` implements fallback data caches, ensuring the UI renders seamlessly even during local backend restarts.

