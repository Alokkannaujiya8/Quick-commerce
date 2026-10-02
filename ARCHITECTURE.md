# QuickCart Architecture Overview

QuickCart is an enterprise-grade **10-minute quick commerce (instant grocery delivery)** platform. It is architected as a **.NET 10 Modular Monolith** structured around **Domain-Driven Design (DDD)** and **Clean Architecture**, with isolated PostgreSQL schemas per domain module.

---

## 1. System Overview

QuickCart addresses the challenges of high-velocity micro-fulfillment:
* **Hyperlocal Dark Store Model**: Orders are serviced from localized dark stores (micro-fulfillment centers) covering a 3–5 km radius.
* **Sub-Minute Dispatch**: Automated order validation, stock reservation, and rider assignment.
* **Real-Time Telemetry**: Real-time rider location updates and order tracking powered by ASP.NET Core SignalR WebSockets.
* **Idempotent High-Concurrency Checkout**: Server-side price calculation, inventory movements, and cryptographic payment verification.

---

## 2. Modular Monolith Architecture

The backend operates as a single deployable host (`QuickCart.Api`) housing 8 autonomous business modules. Each module encapsulates its own business domain, application services, data access layer, and HTTP presentation endpoints.

```text
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

### Module Registry

| Module | Schema | Primary Aggregates | Current Implementation Status |
| :--- | :--- | :--- | :--- |
| **Catalog** | `catalog` | `Product`, `Category`, `SubCategory`, `Brand` | Implemented (Products, Categories APIs) |
| **Inventory** | `inventory` | `DarkStore`, `StoreInventory`, `StockMovement` | Implemented (Dark stores, Geolocation serviceability) |
| **Ordering** | `ordering` | `Order`, `OrderItem`, `OrderStatusHistory` | Implemented (Checkout, Order history, Status machine) |
| **Cart** | `cart` | `Cart`, `CartItem` | Implemented (Get, Add, Update, Remove, Clear) |
| **Delivery** | `delivery` | `DeliveryPartner`, `DeliveryAssignment`, `DeliveryTracking` | Implemented (Rider assignment, ETA calculation, SignalR tracking) |
| **Identity** | `identity` | `ApplicationUser`, `RefreshToken`, `ExternalLogin` | Implemented (JWT, Refresh tokens, BCrypt, Google OAuth, OTP) |
| **Payment** | `payment` | `Payment`, `PaymentAuditLog`, `PaymentWebhookEvent`, `Wallet` | Implemented (Intents, Verification, Webhooks, Refunds, Idempotency) |
| **Promotion** | `promotion` | `Coupon`, `Offer` | Partially Implemented (Domain entities & DbContext; Application & Presentation pending) |

---

## 3. Dependency Direction & Layering

Each module adheres to strict inward Clean Architecture dependency rules:

```text
[ Domain ] <── [ Application ] <── [ Infrastructure ]
                               <── [ Presentation ]
```

* **Domain**: Zero external dependencies. Holds entities, aggregate roots, value objects, domain events, and business invariants.
* **Application**: Holds use case contracts, DTOs, service interfaces, and CQRS handlers. Depends only on Domain and `BuildingBlocks.Application`.
* **Infrastructure**: Implements data access via EF Core 10 (`DbContext`), repository interfaces, and external clients. Implements interfaces defined in Application.
* **Presentation**: Exposes API controllers (`ControllerBase`) and SignalR hubs. Dispatches requests to application services.

---

## 4. Key Request Flows

### 4.1 Order Placement & Checkout Flow
1. Client calls `POST /api/orders/checkout` with `CheckoutRequest` (containing delivery address ID, payment method, and optional items).
2. `OrdersController` delegates to `IOrderService.CreateOrderAsync`.
3. `OrderService` generates a unique `OrderNumber` (`QC-yyyyMMdd-XXXX`), calculates totals and delivery fees server-side, sets status to `Placed`, appends initial `OrderStatusHistory`, and persists to `ordering.Orders`.
4. Returns `OrderDto` (201 Created).

### 4.2 Payment Verification & Confirmation Flow
1. Client calls `POST /api/payments/intents` with `orderId` and `idempotencyKey`.
2. `PaymentService` queries `IOrderService.GetOrderByIdAsync`, verifies caller ownership, fetches the authoritative server-side amount, and creates a payment intent in `payment.Payments`.
3. Client completes payment with the gateway and submits payment details to `POST /api/payments/verify`.
4. `PaymentGatewayService` validates the payment signature using constant-time HMAC-SHA256 (`CryptographicOperations.FixedTimeEquals`).
5. On success, `PaymentService` marks payment as `Captured`, records an immutable entry in `payment.PaymentAuditLogs`, and calls `IOrderService.UpdateOrderStatusAsync(orderId, "Confirmed")`.

### 4.3 Real-Time Delivery Tracking Flow
1. Client connects via WebSocket to `/hubs/delivery-tracking` (`DeliveryTrackingHub`).
2. Client invokes `JoinOrderTrackingGroup(orderId)`.
3. As the order status transitions (`Placed` -> `Confirmed` -> `Packed` -> `OutForDelivery` -> `Delivered`), events (`ReceiveOrderStatusUpdate`) are broadcast to the order group.
4. When `OutForDelivery`, `ReceiveRiderLocationUpdate` streams rider coordinates (`Latitude`, `Longitude`) and dynamic ETA minutes to the client.

---

## 5. Authentication & Authorization Flow

1. **Credentials Login / Registration**: Client submits email/phone and password to `POST /api/auth/login` or `POST /api/auth/register`. Passwords are verified with BCrypt (`BCrypt.Net-Next`).
2. **Google Sign-In**: Client submits Google ID token to `POST /api/auth/google`. Verified via `Google.Apis.Auth` (`GoogleJsonWebSignature`) with clock tolerance and audience validation.
3. **Dual-Token Issuance**: Server returns a short-lived JWT Access Token (15-minute lifetime) and a cryptographically secure random Refresh Token (30-day lifetime).
4. **Refresh Token Storage**: The SHA-256 hash of the refresh token is stored in `identity.RefreshTokens`.
5. **Token Refresh**: Client calls `POST /api/auth/refresh`. Server validates the token hash, issues a new token pair, and rotates the refresh token.
6. **API Protection**: Protected endpoints enforce `[Authorize]` with standard ASP.NET Core JWT Bearer authentication.

---

## 6. Database Architecture

* **Engine**: PostgreSQL 16+ via `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.3.
* **Multi-Schema Isolation**: Each module has its own isolated schema (`catalog`, `inventory`, `ordering`, `cart`, `delivery`, `identity`, `payment`, `promotion`).
* **No Cross-Schema Foreign Keys**: Foreign keys across schemas are strictly prohibited; relationships are maintained logically by `Guid`.
* **Auditing**: `AuditableEntitySaveChangesInterceptor` automatically populates `CreatedAt` and `UpdatedAt` timestamps in UTC for all `IAuditableEntity` models upon save.
* **Migrations**: Automated via `./scripts/migrate-database.ps1` or `tools/QuickCart.DatabaseMigrator`.

---

## 7. Cross-Cutting Capabilities & Non-Functional Areas

| Capability | Current Status | Architecture Summary |
| :--- | :--- | :--- |
| **Error Handling** | Implemented | RFC 7807 Problem Details via `GlobalExceptionHandler` and `IExceptionHandler`. |
| **API Documentation** | Implemented | ASP.NET Core OpenAPI (`Microsoft.AspNetCore.OpenApi`) with Scalar UI at `/scalar/v1`. |
| **Caching** | Status: Not Implemented / Not Confirmed | No backend cache abstractions (IMemoryCache, Redis, OutputCache) are currently configured. |
| **Background Workers** | Status: Not Implemented / Not Confirmed | No `IHostedService`, `BackgroundService`, or background queues are currently implemented. |
| **Multi-Tenancy** | Status: Not Implemented / Not Confirmed | Single-tenant deployment model. No tenant partitioning is present in code or database. |
| **Cloud Deployment** | Status: Not Implemented / Not Confirmed | Local development scripts exist; Docker/Kubernetes/CI-CD are not currently present. |

---

## 8. Detailed Documentation Links

* **Engineering Standards**:
  * [Engineering Rules](docs/engineering/RULES.md)
  * [Technology Stack](docs/engineering/TECH_STACK.md)
  * [Package Dependencies](docs/engineering/DEPENDENCIES.md)
  * [Error Handling & Problem Details](docs/engineering/ERROR_HANDLING.md)
  * [Security & Auth](docs/engineering/SECURITY.md)
  * [Performance Practices](docs/engineering/PERFORMANCE.md)
  * [Testing Strategy](docs/engineering/TESTING.md)
  * [Observability & Telemetry](docs/engineering/OBSERVABILITY.md)
  * [AI Development Boundaries](docs/engineering/AI_BOUNDARIES.md)
* **Architecture Specifications**:
  * [Architecture Specification Index](docs/architecture/README.md)
  * [Solution Structure](docs/architecture/solution-structure.md)
  * [API Layer](docs/architecture/api.md)
  * [Application Layer](docs/architecture/application.md)
  * [Domain Layer](docs/architecture/domain.md)
  * [Infrastructure Layer](docs/architecture/infrastructure.md)
  * [Database Architecture](docs/architecture/database.md)
  * [Domain Modules](docs/architecture/modules.md)
  * [Authentication & Authorization](docs/architecture/authentication-authorization.md)
  * [Multi-Tenancy](docs/architecture/multi-tenancy.md)
  * [Caching](docs/architecture/caching.md)
  * [Integrations](docs/architecture/integrations.md)
  * [Background Workers](docs/architecture/background-workers.md)
  * [Deployment](docs/architecture/deployment.md)
  * [Architecture Diagrams](docs/architecture/diagrams/)
* **Architecture Decision Records (ADRs)**:
  * [ADR Index](docs/adr/README.md)
* **AI & Operational Memory**:
  * [AI Workflow](docs/ai/WORKFLOW.md)
  * [Current State](docs/ai/CURRENT_STATE.md)
  * [Engineering Memory](docs/ai/ENGINEERING_MEMORY.md)
  * [Decisions Index](docs/ai/DECISIONS.md)

