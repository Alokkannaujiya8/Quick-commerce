# Domain Modules Breakdown & Implementation Status

This document provides a granular analysis of all 8 domain modules in the QuickCart modular monolith, including schema boundaries, domain aggregates, services, controllers, and exact verified implementation status.

---

## 1. Summary Matrix

| Module | Schema | Layer Structure | Endpoints Active | Tests Active | Status |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Catalog** | `catalog` | Domain, App, Infra, Presentation | Yes | Skeleton | **Fully Implemented** |
| **Inventory** | `inventory` | Domain, App, Infra, Presentation | Yes | Skeleton | **Fully Implemented** |
| **Ordering** | `ordering` | Domain, App, Infra, Presentation | Yes | Skeleton | **Fully Implemented** |
| **Cart** | `cart` | Domain, App, Infra, Presentation | Yes | *None* | **Fully Implemented** |
| **Delivery** | `delivery` | Domain, App, Infra, Presentation | Yes | Skeleton | **Fully Implemented** |
| **Identity** | `identity` | Domain, App, Infra, Presentation | Yes | **41 Tests** | **Fully Implemented** |
| **Payment** | `payment` | Domain, App, Infra, Presentation | Yes | **14 Tests** | **Fully Implemented** |
| **Promotion** | `promotion` | Domain, Infra (App/Pres empty) | None | *None* | **Partially Implemented** |

---

## 2. Granular Module Specifications

### 🏷️ 2.1 Catalog Module (`src/Modules/Catalog/`)
* **Schema**: `catalog`
* **Role**: Master product catalog, taxonomic hierarchy, brands, and packaging units.
* **Key Aggregates**:
  * `Product` (`Id`, `Name`, `Slug`, `Description`, `SubCategoryId`, `BrandId`, `SKU`, `UnitOfMeasure`, `ImageUrl`, `IsActive`)
  * `Category` (`Id`, `Name`, `Slug`, `IconUrl`, `DisplayOrder`, `IsActive`)
  * `SubCategory` (`Id`, `CategoryId`, `Name`, `Slug`, `DisplayOrder`, `IsActive`)
  * `Brand` (`Id`, `Name`, `Description`, `LogoUrl`, `IsActive`)
* **Services**: `ICatalogService`, `CatalogService`
* **Endpoints**: `ProductsController`
  * `GET /api/catalog/categories`
  * `GET /api/catalog/products`
  * `GET /api/catalog/products/{slug}`

---

### 🏬 2.2 Inventory Module (`src/Modules/Inventory/`)
* **Schema**: `inventory`
* **Role**: Hyperlocal dark store registry, store-level inventory balances, and spatial serviceability matching.
* **Key Aggregates**:
  * `DarkStore` (`Id`, `Name`, `Code`, `Address`, `Latitude`, `Longitude`, `ServiceRadiusKm`, `IsActive`)
  * `StoreInventory` (`Id`, `DarkStoreId`, `ProductId`, `AvailableQuantity`, `ReservedQuantity`, `ReorderThreshold`)
  * `StockMovement` (`Id`, `StoreInventoryId`, `QuantityChanged`, `MovementType`, `Reason`, `CreatedAt`)
* **Services**: `IInventoryService`, `InventoryService`, `GeoLocationService`
* **Endpoints**: `DarkStoresController`
  * `GET /api/dark-stores`
  * `POST /api/serviceability` (matches customer GPS coordinates against dark store service radii)

---

### 📦 2.3 Ordering Module (`src/Modules/Ordering/`)
* **Schema**: `ordering`
* **Role**: Order placement, server-side pricing, price snapshots at checkout, and fulfillment lifecycle tracking.
* **Key Aggregates**:
  * `Order` (`Id`, `OrderNumber`, `UserId`, `DeliveryAddressId`, `Status`, `Subtotal`, `DeliveryFee`, `DiscountAmount`, `TotalAmount`, `PlacedAt`, `DeliveredAt`, `CancelledAt`, `CancellationReason`)
  * `OrderItem` (`Id`, `OrderId`, `ProductId`, `ProductNameSnapshot`, `ProductSkuSnapshot`, `UnitPrice`, `Quantity`, `LineTotal`)
  * `OrderStatusHistory` (`Id`, `OrderId`, `Status`, `ChangedAt`, `Notes`)
* **Enums**: `OrderStatus` (`Placed`, `Confirmed`, `Packed`, `OutForDelivery`, `Delivered`, `Cancelled`, `Returned`)
* **Services**: `IOrderService`, `OrderService`
* **Endpoints**: `OrdersController`
  * `POST /api/orders/checkout`
  * `GET /api/orders` (retrieves orders for authenticated caller)
  * `GET /api/orders/{id}`
  * `POST /api/orders/{id}/status`

---

### 🛒 2.4 Cart Module (`src/Modules/Cart/`)
* **Schema**: `cart`
* **Role**: Shopping cart management, quantity increments/decrements, subtotal derivation.
* **Key Aggregates**:
  * `Cart` (`Id`, `UserId`, `CreatedAt`, `UpdatedAt`)
  * `CartItem` (`Id`, `CartId`, `ProductId`, `Quantity`)
* **Services**: `CartService`
* **Endpoints**: `CartController`
  * `GET /api/cart`
  * `POST /api/cart/items`
  * `PUT /api/cart/items/{productId}`
  * `DELETE /api/cart/items/{productId}`
  * `DELETE /api/cart`

---

### 🛵 2.5 Delivery Module (`src/Modules/Delivery/`)
* **Schema**: `delivery`
* **Role**: Rider fleet dispatch, dynamic ETA estimation, and real-time GPS telemetry.
* **Key Aggregates**:
  * `DeliveryPartner` (`Id`, `FullName`, `PhoneNumber`, `VehicleType`, `Status`, `CurrentLatitude`, `CurrentLongitude`)
  * `DeliveryAssignment` (`Id`, `OrderId`, `DeliveryPartnerId`, `AssignedAt`, `PickedUpAt`, `DeliveredAt`, `Status`)
  * `DeliveryTracking` (`Id`, `DeliveryAssignmentId`, `Latitude`, `Longitude`, `RecordedAt`)
* **Enums**: `DeliveryPartnerStatus` (`Available`, `Busy`, `Offline`)
* **Services**: `IDeliveryService`, `DeliveryService`, `ETACalculationService`
* **Endpoints & Hubs**:
  * `DeliveryController`:
    * `POST /api/orders/{orderId}/assign`
    * `GET /api/orders/{orderId}/tracking`
  * `DeliveryTrackingHub` (`/hubs/delivery-tracking`): Real-time SignalR WebSocket hub for live rider location and order state broadcasts.

---

### 👤 2.6 Identity Module (`src/Modules/Identity/`)
* **Schema**: `identity`
* **Role**: User accounts, credentials, BCrypt password hashing, Google OAuth, and rotating refresh tokens.
* **Key Aggregates**:
  * `ApplicationUser` (`Id`, `FullName`, `Email`, `PhoneNumber`, `PasswordHash`, `Status`, `EmailVerified`, `PhoneNumberVerified`, `ProfilePictureUrl`)
  * `RefreshToken` (`Id`, `UserId`, `TokenHash`, `DeviceName`, `ExpiresAt`, `RevokedAt`, `CreatedAt`)
  * `ExternalLogin` (`Id`, `UserId`, `Provider`, `ProviderUserId`, `Email`, `LastLoginAt`)
* **Services**: `IIdentityService`, `IdentityService`, `IJwtTokenService`, `JwtTokenService`, `IGoogleTokenValidator`, `GoogleTokenValidator`
* **Repositories**: `IUserRepository`, `UserRepository`
* **Endpoints**: `AuthController`
  * `POST /api/auth/register`
  * `POST /api/auth/login`
  * `POST /api/auth/otp/send`
  * `POST /api/auth/otp/verify`
  * `POST /api/auth/google`
  * `POST /api/auth/refresh`
  * `POST /api/auth/logout`
  * `GET /api/auth/me`

---

### 💳 2.7 Payment Module (`src/Modules/Payment/`)
* **Schema**: `payment`
* **Role**: Payment intents, server-verified amounts, HMAC-SHA256 signature checks, webhook deduplication, and refunds.
* **Key Aggregates**:
  * `Payment` (`Id`, `OrderId`, `UserId`, `Amount`, `RefundedAmount`, `Currency`, `Method`, `Status`, `ProviderName`, `ProviderOrderId`, `ProviderPaymentId`, `IdempotencyKey`, `FailureReason`, `RetryCount`)
  * `PaymentAuditLog` (`Id`, `PaymentId`, `EventType`, `PreviousStatus`, `NewStatus`, `ProviderReference`, `Notes`)
  * `PaymentWebhookEvent` (`Id`, `ProviderEventId`, `EventType`, `PaymentId`, `PayloadHash`, `ProcessedAt`)
  * `Wallet` (`Id`, `UserId`, `Balance`, `Currency`)
* **Services**: `IPaymentService`, `PaymentService`, `IPaymentGatewayService`, `PaymentGatewayService`
* **Repositories**: `IPaymentRepository`, `PaymentRepository`
* **Endpoints**: `PaymentsController`
  * `POST /api/payments/intents`
  * `POST /api/payments/verify`
  * `POST /api/payments/webhook`
  * `GET /api/payments/{id}`
  * `GET /api/payments/order/{orderId}`
  * `POST /api/payments/{id}/retry`
  * `POST /api/payments/{id}/refund`

---

### 🎟️ 2.8 Promotion Module (`src/Modules/Promotion/`)
* **Schema**: `promotion`
* **Role**: Promotional discounts, coupon verification, and marketing offers.
* **Key Aggregates**:
  * `Coupon` (`Id`, `Code`, `DiscountType`, `DiscountValue`, `MinOrderValue`, `MaxDiscountAmount`, `ValidFrom`, `ValidTo`, `UsageLimit`, `TimesUsed`, `IsActive`)
  * `Offer` (`Id`, `Title`, `Description`, `BannerUrl`, `StartDate`, `EndDate`, `IsActive`)
* **Current Implementation State**:
  * `Promotion.Domain`: Contains entity definitions (`Coupon.cs`, `Offer.cs`).
  * `Promotion.Infrastructure`: Contains `PromotionDbContext` and initial EF Core migration (`20260907180846_InitialPromotionSchema`).
  * `Promotion.Application`: 0 source files (Use cases and DTOs not yet defined).
  * `Promotion.Presentation`: 0 source files (Endpoints and controllers not yet implemented).

