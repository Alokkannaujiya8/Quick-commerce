# QuickCart Current State Baseline

This document provides a verified, factual snapshot of the QuickCart backend codebase as of October 2026.

---

## 1. Verified System Architecture

* **Architectural Style**: Modular Monolith following Clean Architecture principles (Onion Architecture / DDD).
* **Composition Root**: `src/QuickCart.Api` (ASP.NET Core 10 Web API host).
* **Foundation**: `src/BuildingBlocks` (Domain, Application, Infrastructure, Presentation).
* **Runtime**: .NET 10.0 (`net10.0`), C# 14.0, SDK 10.0.401.
* **Database**: PostgreSQL 16+ via `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.3 with 8 isolated schemas (`catalog`, `inventory`, `ordering`, `cart`, `delivery`, `identity`, `payment`, `promotion`).
* **Real-Time Layer**: SignalR WebSocket hub at `/hubs/delivery-tracking`.
* **API Documentation**: Scalar interactive documentation UI at `/scalar/v1` (OpenAPI v3).

---

## 2. Module Implementation Status Matrix

| Module | Schema | Entities Present | Application Services | API Controllers | Status |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Catalog** | `catalog` | 4 entities | `CatalogService` | `ProductsController` (3 endpoints) | **Fully Implemented** |
| **Inventory** | `inventory` | 3 entities | `InventoryService`, `GeoLocationService` | `DarkStoresController` (2 endpoints) | **Fully Implemented** |
| **Ordering** | `ordering` | 3 entities | `OrderService` | `OrdersController` (4 endpoints) | **Fully Implemented** |
| **Cart** | `cart` | 2 entities | `CartService` | `CartController` (5 endpoints) | **Fully Implemented** |
| **Delivery** | `delivery` | 3 entities | `DeliveryService`, `ETACalculationService` | `DeliveryController`, `DeliveryTrackingHub` | **Fully Implemented** |
| **Identity** | `identity` | 3 entities | `IdentityService`, `JwtTokenService`, `GoogleTokenValidator` | `AuthController` (8 endpoints) | **Fully Implemented** |
| **Payment** | `payment` | 4 entities | `PaymentService`, `PaymentGatewayService` | `PaymentsController` (7 endpoints) | **Fully Implemented** |
| **Promotion** | `promotion` | 2 entities | *None (0 files)* | *None (0 files)* | **Partially Implemented** |

---

## 3. Test Suites & Verification Status

Solution test execution (`dotnet test QuickCart.sln`):
* **Total Executed Tests**: **55**
* **Passed**: **55**
* **Failed**: **0**
* **Skipped**: **0**

### Test Breakdown by Project
1. **`tests/Identity.Tests`**: **41 Passed** (Application, Domain, Infrastructure Google & JWT token validation, Presentation Google auth).
2. **`tests/Payment.Tests`**: **14 Passed** (Payment intents, server verification, IDOR checks, HMAC signatures, webhooks, refunds).
3. **`tests/Catalog.Tests`**: *Unconfigured skeleton* (No test packages or test classes).
4. **`tests/Delivery.Tests`**: *Unconfigured skeleton* (No test packages or test classes).
5. **`tests/Inventory.Tests`**: *Unconfigured skeleton* (No test packages or test classes).
6. **`tests/Ordering.Tests`**: *Unconfigured skeleton* (No test packages or test classes).
7. **`tests/QuickCart.IntegrationTests`**: *Unconfigured skeleton* (No test packages or test classes).

---

## 4. Build Status

Command: `dotnet build QuickCart.sln --no-incremental`
* **Result**: **Build succeeded**
* **Warnings**: 0
* **Errors**: 0
* **Projects Compiled**: 46 projects.

---

## 5. Known Gaps & Unimplemented Architectural Areas

1. **Addresses Entity Gap**:
   * Initial documentation described an `identity.Addresses` table. Audit of `IdentityDbContext` confirms only `Users`, `RefreshTokens`, and `ExternalLogins` exist. Order delivery destinations currently rely on logical `DeliveryAddressId` (`Guid`).
2. **Promotion Module Completion**:
   * `Promotion.Application` and `Promotion.Presentation` contain 0 source files. Use cases, coupon validation handlers, and promotion endpoints remain unimplemented.
3. **Test Project Configuration**:
   * 5 out of 7 test projects need xUnit, Moq, and Test SDK package references and initial test suites.
4. **Caching**:
   * Backend has zero caching layers (IMemoryCache, Redis, or OutputCache). All reads hit PostgreSQL directly.
5. **Background Processing**:
   * All tasks (including webhook processing and order confirmation) execute synchronously in-process. No `BackgroundService` or task queues exist.
6. **Telemetry & Tracing**:
   * No correlation ID tracking or OpenTelemetry metrics/traces are configured.
7. **Containerization & CI/CD**:
   * No Dockerfiles, docker-compose, or CI/CD pipelines exist in the repository.

