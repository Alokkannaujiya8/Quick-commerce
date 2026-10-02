# Domain Layer Architecture

This document details the Domain-Driven Design (DDD) foundation, entity models, aggregate roots, value objects, domain events, and business invariants across the QuickCart domain modules.

---

## 1. Domain Layer Philosophy

The domain layer (`<Module>.Domain`) is the core of the application. It captures business rules, invariants, lifecycle states, and domain models:
* **Zero External Dependencies**: Depends only on `BuildingBlocks.Domain`.
* **Framework Agnostic**: Does not reference EF Core, ASP.NET Core, serializers, or data access tooling.
* **Schema Ownership**: Models the business entities mapped to the module's dedicated PostgreSQL schema.

---

## 2. Shared Domain Primitives (`BuildingBlocks.Domain`)

### 2.1 Entity Base Classes
* **[`Entity<TId>`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/BuildingBlocks/BuildingBlocks.Domain/Entities/Entity.cs)**: Base entity class encapsulating identifier equality (`operator ==`, `operator !=`, `Equals`, `GetHashCode`).
* **[`AuditableEntity<TId>`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/BuildingBlocks/BuildingBlocks.Domain/Entities/AuditableEntity.cs)**: Extends `Entity<TId>` and implements `IAuditableEntity`, providing automatic audit timestamps:
  * `DateTime CreatedAt { get; set; }`
  * `DateTime? UpdatedAt { get; set; }`

### 2.2 Domain Contracts
* **[`IAuditableEntity`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/BuildingBlocks/BuildingBlocks.Domain/Contracts/IAuditableEntity.cs)**: Marker contract for entities whose creation and modification dates are automatically tracked in UTC by `AuditableEntitySaveChangesInterceptor`.
* **[`IDomainEvent`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/BuildingBlocks/BuildingBlocks.Domain/Contracts/IDomainEvent.cs)**: Foundation contract for domain events representing business occurrences within aggregates.

---

## 3. Module Aggregate Roots & Entities

| Module | Primary Aggregates & Entities | Enums & Value Types | Core Domain Invariants |
| :--- | :--- | :--- | :--- |
| **Catalog** | `Product`, `Category`, `SubCategory`, `Brand` | *None* | Products must have unique `SKU` and SEO `Slug`. Categories and Subcategories must have unique `Slug`. |
| **Inventory** | `DarkStore`, `StoreInventory`, `StockMovement` | `MovementType` | Stock cannot fall below zero. Every stock change produces an immutable `StockMovement` ledger entry. |
| **Ordering** | `Order`, `OrderItem`, `OrderStatusHistory` | `OrderStatus` (`Placed`, `Confirmed`, `Packed`, `OutForDelivery`, `Delivered`, `Cancelled`, `Returned`) | Prices and product names are frozen as immutable snapshots at purchase time (`ProductNameSnapshot`, `UnitPrice`). State transitions append to `OrderStatusHistories`. |
| **Cart** | `Cart`, `CartItem` | *None* | Item quantities must be positive integers. Items reference products via `Guid ProductId`. |
| **Delivery** | `DeliveryPartner`, `DeliveryAssignment`, `DeliveryTracking` | `DeliveryPartnerStatus` (`Available`, `Busy`, `Offline`) | Only active/available riders can be assigned to orders. Live GPS telemetry records timestamped route trails. |
| **Identity** | `ApplicationUser`, `RefreshToken`, `ExternalLogin` | `UserStatus` (`Active`, `Suspended`, `PendingVerification`) | Emails and phone numbers must be unique across the platform. Passwords must be hashed with BCrypt. Refresh tokens stored only as SHA-256 hashes. |
| **Payment** | `Payment`, `PaymentAuditLog`, `PaymentWebhookEvent`, `Wallet` | `PaymentStatus`, `PaymentMethodType` | Authoritative amounts must match order totals. State changes create immutable `PaymentAuditLog` entries. Webhooks deduplicate on `ProviderEventId`. |
| **Promotion** | `Coupon`, `Offer` | *None* | Defines discount codes, minimum order value thresholds, discount percentages, and validity dates. |

---

## 4. Cross-Module Boundaries & Logical References

Cross-module entity references are strictly maintained by logical `Guid` values. Direct navigation properties across domain modules are prohibited:

```csharp
// Example from Ordering.Domain.Entities.Order
public class Order : AuditableEntity<Guid>
{
    public string OrderNumber { get; set; } = string.Empty;
    public Guid UserId { get; set; }           // Logical reference to Identity.Users
    public Guid DeliveryAddressId { get; set; } // Logical reference to delivery destination
    public OrderStatus Status { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<OrderStatusHistory> OrderStatusHistories { get; set; } = new List<OrderStatusHistory>();
}
```
This design maintains complete autonomy between module boundaries, enabling future extraction into microservices if needed.

