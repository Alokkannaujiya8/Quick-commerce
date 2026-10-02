# Database Architecture & Schema Specification

This document details the PostgreSQL multi-schema database architecture, entity-to-table mappings, index strategies, and migration management across the QuickCart backend.

---

## 1. Architectural Foundation

* **Database Engine**: PostgreSQL 16+
* **ORM**: Entity Framework Core 10 (`Microsoft.EntityFrameworkCore` `10.0.4`)
* **Npgsql Provider**: `Npgsql.EntityFrameworkCore.PostgreSQL` (`10.0.3`)
* **Isolation Pattern**: **Multi-Schema Database Isolation**. Each domain module operates inside an isolated PostgreSQL schema within the shared database (`quickcartdb`).
* **Cross-Schema Isolation Rules**:
  * Cross-schema physical foreign keys are strictly prohibited.
  * Cross-module entity references are stored exclusively as immutable `Guid` values.
  * Schema ownership is bounded per `DbContext`.

---

## 2. Schema & Table Directory

| Schema | Module DbContext | Mapped Tables | Primary Responsibility |
| :--- | :--- | :--- | :--- |
| **`catalog`** | `CatalogDbContext` | `Products`, `Categories`, `SubCategories`, `Brands` | Master catalog, taxonomy, and brand definitions |
| **`inventory`** | `InventoryDbContext` | `DarkStores`, `StoreInventories`, `StockMovements` | Hyperlocal dark store inventory balances and movement ledger |
| **`ordering`** | `OrderingDbContext` | `Orders`, `OrderItems`, `OrderStatusHistories` | Customer orders, line items, and lifecycle timeline |
| **`cart`** | `CartDbContext` | `Carts`, `CartItems` | Ephemeral user shopping carts |
| **`delivery`** | `DeliveryDbContext` | `DeliveryPartners`, `DeliveryAssignments`, `DeliveryTrackings` | Delivery riders, dispatch assignments, and GPS tracking |
| **`identity`** | `IdentityDbContext` | `Users`, `RefreshTokens`, `ExternalLogins` | User credentials, hashed refresh tokens, and OAuth logins |
| **`payment`** | `PaymentDbContext` | `Payments`, `PaymentAuditLogs`, `PaymentWebhookEvents`, `Wallets` | Payment transactions, audit ledger, and webhook deduplication |
| **`promotion`** | `PromotionDbContext` | `Coupons`, `Offers` | Promotional codes and campaign discounts |

> [!NOTE]
> **Address Entity Audit**: Earlier drafts referenced an `identity.Addresses` table. An audit of [`IdentityDbContext.cs`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/Modules/Identity/Identity.Infrastructure/Persistence/IdentityDbContext.cs) confirms `DbSet<Address>` is not configured. Delivery destination references currently persist as `DeliveryAddressId` (`uuid`) on `ordering.Orders`.

---

## 3. High-Traffic Indexes & Constraints

### 3.1 `catalog` Schema
* `catalog.Products`:
  * `PK_Products`: `Id` (uuid)
  * `IX_Products_Slug`: Unique index on `Slug`
  * `IX_Products_SKU`: Unique index on `SKU`
  * `IX_Products_SubCategoryId`: Non-unique index on `SubCategoryId`
* `catalog.Categories`: Unique index on `Slug`
* `catalog.SubCategories`: Unique index on `Slug`

### 3.2 `ordering` Schema
* `ordering.Orders`:
  * `PK_Orders`: `Id` (uuid)
  * `IX_Orders_OrderNumber`: Unique index on `OrderNumber`
  * `IX_Orders_UserId`: Non-unique index on `UserId`
* `ordering.OrderItems`:
  * `FK_OrderItems_Orders_OrderId`: Cascade delete on `OrderId`
* `ordering.OrderStatusHistories`:
  * `FK_OrderStatusHistories_Orders_OrderId`: Cascade delete on `OrderId`

### 3.3 `identity` Schema
* `identity.Users`:
  * `IX_Users_Email`: Unique index on `Email`
  * `IX_Users_PhoneNumber`: Unique index on `PhoneNumber`
* `identity.RefreshTokens`:
  * `IX_RefreshTokens_TokenHash`: Unique index on `TokenHash` (SHA-256)
  * `IX_RefreshTokens_UserId`: Non-unique index on `UserId`
* `identity.ExternalLogins`:
  * `IX_ExternalLogins_Provider_ProviderUserId`: Unique composite index

### 3.4 `payment` Schema
* `payment.Payments`:
  * `IX_Payments_ProviderOrderId`: Unique index on `ProviderOrderId`
  * `IX_Payments_IdempotencyKey`: Unique index on `IdempotencyKey`
  * `IX_Payments_OrderId`: Non-unique index on `OrderId`
* `payment.PaymentWebhookEvents`:
  * `IX_PaymentWebhookEvents_ProviderEventId`: Unique index on `ProviderEventId`
* `payment.PaymentAuditLogs`:
  * `FK_PaymentAuditLogs_Payments_PaymentId`: Cascade delete on `PaymentId`

---

## 4. Migration Execution & Tooling

Each module owns an independent EF Core migration history.

### 4.1 Migration Runner Script (`scripts/migrate-database.ps1`)
Applies pending migrations across all 8 schemas in sequence:
```powershell
./scripts/migrate-database.ps1
```
Or for a specific module:
```powershell
./scripts/migrate-database.ps1 -Module Ordering
```

### 4.2 Dedicated Migration CLI (`tools/QuickCart.DatabaseMigrator`)
A compiled console utility that executes pending migrations across all registered DbContexts programmatically:
```powershell
dotnet run --project tools/QuickCart.DatabaseMigrator
```

