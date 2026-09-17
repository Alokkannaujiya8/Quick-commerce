# QuickCart Database Specification

This document details the PostgreSQL database architecture, schema isolation strategies, entity configurations, indexes, and migration guidelines for **QuickCart**.

---

## 1. Database Overview

* **Engine**: PostgreSQL 16+
* **ORM**: Entity Framework Core 10 (`Npgsql.EntityFrameworkCore.PostgreSQL`)
* **Default Database**: `quickcartdb`
* **Architecture**: **Multi-Schema Database Isolation**. Each domain module operates inside its own isolated database schema.
* **Cross-Schema Integrity**: Physical foreign keys across schemas are strictly forbidden. Cross-module references are maintained logically via immutable `Guid` keys to ensure that any module can be extracted into an independent database in the future.

---

## 2. Schema Breakdown

| Schema | Module DbContext | Primary Tables | Responsibility |
| :--- | :--- | :--- | :--- |
| `catalog` | `CatalogDbContext` | `Products`, `Categories`, `SubCategories`, `Brands` | Master catalog and taxonomy |
| `inventory` | `InventoryDbContext` | `DarkStores`, `StoreInventories`, `StockMovements` | Hyperlocal store inventory & stock movement ledger |
| `ordering` | `OrderingDbContext` | `Orders`, `OrderItems`, `OrderStatusHistories` | Orders, price snapshots, order timeline |
| `cart` | `CartDbContext` | `Carts`, `CartItems` | Customer shopping carts |
| `delivery` | `DeliveryDbContext` | `DeliveryPartners`, `DeliveryAssignments`, `DeliveryTrackings` | Riders, fulfillment dispatch, GPS tracking |
| `identity` | `IdentityDbContext` | `Users`, `Addresses` | User accounts, credentials, delivery addresses |
| `payment` | `PaymentDbContext` | `Payments`, `Wallets` | Payment transactions, wallet ledger |
| `promotion` | `PromotionDbContext` | `Coupons`, `Offers` | Discount rules and campaigns |

---

## 3. Schema & Table Specifications

### 🏷️ Schema: `catalog`

#### Table: `catalog.Products`
* `Id` (`uuid`, PK): Product primary identifier.
* `Name` (`varchar(300)`, NOT NULL): Display name.
* `Slug` (`varchar(300)`, NOT NULL, UNIQUE INDEX): SEO-friendly slug.
* `Description` (`varchar(2000)`, NULL): Full description.
* `SubCategoryId` (`uuid`, NOT NULL, FK to `catalog.SubCategories.Id`): Subcategory link.
* `BrandId` (`uuid`, NULL, FK to `catalog.Brands.Id`): Optional brand link.
* `SKU` (`varchar(100)`, NOT NULL, UNIQUE INDEX): Stock keeping unit.
* `UnitOfMeasure` (`varchar(50)`, NOT NULL): Packaging size (e.g. `500 ml`, `1 kg`, `6 pcs`).
* `ImageUrl` (`varchar(500)`, NULL): Product primary image.
* `IsActive` (`boolean`, DEFAULT true): Catalog availability flag.
* `CreatedAt` (`timestamptz`, NOT NULL): Creation audit timestamp.
* `UpdatedAt` (`timestamptz`, NULL): Update timestamp.

#### Table: `catalog.Categories`
* `Id` (`uuid`, PK)
* `Name` (`varchar(200)`, NOT NULL)
* `Slug` (`varchar(200)`, NOT NULL, UNIQUE INDEX)
* `IconUrl` (`varchar(500)`, NULL)
* `DisplayOrder` (`integer`, DEFAULT 0)
* `IsActive` (`boolean`, DEFAULT true)
* `CreatedAt` (`timestamptz`, NOT NULL)

#### Table: `catalog.SubCategories`
* `Id` (`uuid`, PK)
* `CategoryId` (`uuid`, NOT NULL, FK to `catalog.Categories.Id`)
* `Name` (`varchar(200)`, NOT NULL)
* `Slug` (`varchar(200)`, NOT NULL, UNIQUE INDEX)
* `DisplayOrder` (`integer`, DEFAULT 0)
* `IsActive` (`boolean`, DEFAULT true)
* `CreatedAt` (`timestamptz`, NOT NULL)

#### Table: `catalog.Brands`
* `Id` (`uuid`, PK)
* `Name` (`varchar(200)`, NOT NULL)
* `Description` (`varchar(1000)`, NULL)
* `LogoUrl` (`varchar(500)`, NULL)
* `IsActive` (`boolean`, DEFAULT true)
* `CreatedAt` (`timestamptz`, NOT NULL)

---

### 📦 Schema: `ordering`

#### Table: `ordering.Orders`
* `Id` (`uuid`, PK): Order identifier.
* `OrderNumber` (`varchar(30)`, NOT NULL, UNIQUE INDEX): Human-readable identifier (e.g. `QC-202609-8472`).
* `UserId` (`uuid`, NOT NULL, INDEX): Customer identity reference.
* `DeliveryAddressId` (`uuid`, NOT NULL): Delivery address snapshot.
* `Status` (`varchar(50)`, NOT NULL, DEFAULT 'Placed'): String conversion of `OrderStatus`.
* `Subtotal` (`numeric(10,2)`, NOT NULL): Gross items total.
* `DeliveryFee` (`numeric(10,2)`, DEFAULT 0.00): Delivery fee.
* `DiscountAmount` (`numeric(10,2)`, DEFAULT 0.00): Applied promotional discounts.
* `TotalAmount` (`numeric(10,2)`, NOT NULL): Final payable total.
* `PlacedAt` (`timestamptz`, NOT NULL): Timestamp when order was submitted.
* `DeliveredAt` (`timestamptz`, NULL): Delivery timestamp.
* `CancelledAt` (`timestamptz`, NULL): Cancellation timestamp.
* `CancellationReason` (`varchar(500)`, NULL): Cancellation notes.
* `CreatedAt` (`timestamptz`, NOT NULL): Audit creation timestamp.

#### Table: `ordering.OrderItems`
* `Id` (`uuid`, PK)
* `OrderId` (`uuid`, NOT NULL, FK to `ordering.Orders.Id`, CASCADE DELETE)
* `ProductId` (`uuid`, NOT NULL): Catalog product reference.
* `ProductNameSnapshot` (`varchar(300)`, NOT NULL): Name snapshot at time of purchase.
* `ProductSkuSnapshot` (`varchar(100)`, NOT NULL): SKU snapshot.
* `UnitPrice` (`numeric(10,2)`, NOT NULL): Price snapshot at purchase.
* `Quantity` (`integer`, NOT NULL)
* `LineTotal` (`numeric(10,2)`, NOT NULL)

#### Table: `ordering.OrderStatusHistories`
* `Id` (`uuid`, PK)
* `OrderId` (`uuid`, NOT NULL, FK to `ordering.Orders.Id`, CASCADE DELETE)
* `Status` (`varchar(50)`, NOT NULL): Status state.
* `ChangedAt` (`timestamptz`, NOT NULL): Change timestamp.
* `Notes` (`varchar(500)`, NULL): Optional state transition notes.

---

### 👤 Schema: `identity`

#### Table: `identity.Users`
* `Id` (`uuid`, PK): User identifier.
* `FullName` (`varchar(200)`, NOT NULL): User name.
* `Email` (`varchar(256)`, NOT NULL, UNIQUE INDEX): User email.
* `PhoneNumber` (`varchar(20)`, NOT NULL, UNIQUE INDEX): Mobile phone for OTP.
* `PasswordHash` (`varchar(500)`, NOT NULL): Secure hashed password.
* `IsEmailVerified` (`boolean`, DEFAULT false)
* `IsPhoneVerified` (`boolean`, DEFAULT false)
* `IsActive` (`boolean`, DEFAULT true)
* `CreatedAt` (`timestamptz`, NOT NULL)

#### Table: `identity.Addresses`
* `Id` (`uuid`, PK): Address identifier.
* `UserId` (`uuid`, NOT NULL, FK to `identity.Users.Id`, CASCADE DELETE)
* `Label` (`varchar(50)`, NOT NULL): e.g., "Home", "Work", "Other".
* `AddressLine1` (`varchar(300)`, NOT NULL)
* `AddressLine2` (`varchar(300)`, NULL)
* `City` (`varchar(100)`, NOT NULL)
* `State` (`varchar(100)`, NOT NULL)
* `PostalCode` (`varchar(20)`, NOT NULL)
* `Country` (`varchar(100)`, DEFAULT 'India')
* `Latitude` (`numeric(9,6)`, NOT NULL): Precise coordinate for dark store serviceability matching.
* `Longitude` (`numeric(9,6)`, NOT NULL): Precise coordinate for rider routing.
* `IsDefault` (`boolean`, DEFAULT false)
* `CreatedAt` (`timestamptz`, NOT NULL)

---

## 4. EF Core Migration Guidelines

Each module owns an independent migration stream. Migrations must be created and applied per module.

### Adding a New Migration for a Module
```powershell
# Format:
dotnet ef migrations add <MigrationName> `
    --project src/Modules/<ModuleName>/<ModuleName>.Infrastructure `<ModuleName>.Infrastructure.csproj `
    --startup-project src/QuickCart.Api/QuickCart.Api.csproj `
    --context <ModuleName>DbContext `
    --output-dir Persistence/Migrations

# Example for Catalog:
dotnet ef migrations add AddProductRating `
    --project src/Modules/Catalog/Catalog.Infrastructure/Catalog.Infrastructure.csproj `
    --startup-project src/QuickCart.Api/QuickCart.Api.csproj `
    --context CatalogDbContext `
    --output-dir Persistence/Migrations
```

### Applying Migrations Across All Schemas
Use the automated PowerShell migration runner:
```powershell
./scripts/migrate-database.ps1
```

Or apply a single module:
```powershell
./scripts/migrate-database.ps1 -Module Ordering
```

