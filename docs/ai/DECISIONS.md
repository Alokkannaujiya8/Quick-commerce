# Confirmed Architectural Decisions Index

This document tracks all confirmed architectural decisions governing the QuickCart backend, linking each decision to its corresponding Architecture Decision Record (ADR) and code verification artifacts.

---

## Decision Log

### 1. Hyperlocal Dark Store Serviceability
* **Status**: Confirmed & Approved
* **ADR Link**: [ADR-0001: Hyperlocal Dark Store Serviceability](../adr/ADR-0001-hyperlocal-darkstore-serviceability.md)
* **Summary**: All fulfillment is partitioned across localized dark stores serving a 3–5 km radius. Product availability in the catalog is filtered by the customer's catchment zone using GPS spatial distance calculations.
* **Verified In**: `DarkStore`, `StoreInventory`, `GeoLocationService`, `DarkStoresController`.

---

### 2. 10-Minute Order Fulfillment & Delivery State Machine
* **Status**: Confirmed & Approved
* **ADR Link**: [ADR-0002: 10-Minute Order Fulfillment & Delivery State Machine](../adr/ADR-0002-order-delivery-state-machine.md)
* **Summary**: Orders follow an explicit state machine (`Placed` $\rightarrow$ `Confirmed` $\rightarrow$ `Packed` $\rightarrow$ `OutForDelivery` $\rightarrow$ `Delivered`). Every transition is immutably logged in `ordering.OrderStatusHistories`. Status changes and rider GPS streams broadcast via SignalR WebSockets.
* **Verified In**: `Order`, `OrderStatusHistory`, `OrderService`, `DeliveryTrackingHub`.

---

### 3. Multi-Schema PostgreSQL Isolation without Cross-Schema Foreign Keys
* **Status**: Confirmed & Approved
* **ADR Link**: [ADR-0003: Multi-Schema PostgreSQL Isolation without Cross-Schema Keys](../adr/ADR-0003-multi-schema-database-isolation.md)
* **Summary**: Each domain module operates in an isolated PostgreSQL schema (`catalog`, `inventory`, `ordering`, `cart`, `delivery`, `identity`, `payment`, `promotion`). Foreign keys across schemas are strictly prohibited; references are stored as logical `Guid` values.
* **Verified In**: All 8 `*DbContext.cs` configurations and migration streams.

---

### 4. Dual-Token JWT & Cryptographic Rotating Refresh Tokens
* **Status**: Confirmed & Approved
* **ADR Link**: [ADR-0004: Dual-Token JWT & Cryptographic Rotating Refresh Tokens](../adr/ADR-0004-jwt-refresh-token-authentication.md)
* **Summary**: Authentication uses 15-minute HMAC-SHA256 JWT access tokens and 30-day rotating refresh tokens. The database stores only SHA-256 hashes of refresh tokens. Passwords use BCrypt with automatic salting.
* **Verified In**: `JwtTokenService`, `IdentityService`, `RefreshToken`, `UserRepository`.

---

### 5. Server-Verified Payment Intents, HMAC-SHA256 & Idempotency
* **Status**: Confirmed & Approved
* **ADR Link**: [ADR-0005: Server-Verified Payment Intents, HMAC-SHA256 & Idempotency](../adr/ADR-0005-hmac-sha256-payment-idempotency.md)
* **Summary**: Client payment amounts are never trusted; amounts are verified server-side against `ordering.Orders`. Signature validation uses constant-time HMAC-SHA256 comparison. Inbound webhooks deduplicate on `ProviderEventId`.
* **Verified In**: `PaymentService`, `PaymentGatewayService`, `PaymentWebhookEvent`, `PaymentAuditLog`.

---

### 6. Interactive OpenAPI Documentation via Scalar
* **Status**: Confirmed & Approved
* **Summary**: Interactive API documentation is generated via `Microsoft.AspNetCore.OpenApi` and rendered using `Scalar.AspNetCore` at `/scalar/v1` during development.
* **Verified In**: `QuickCart.Api/Bootstrap/MiddlewarePipeline.cs`.

---

### 7. Global RFC 7807 Problem Details
* **Status**: Confirmed & Approved
* **Summary**: All domain exceptions (`NotFoundException`, `ValidationException`, `ConflictException`, `UnauthorizedAccessException`) are caught centrally by `GlobalExceptionHandler` and rendered as RFC 7807 Problem Details (`application/problem+json`).
* **Verified In**: `BuildingBlocks.Presentation/ExceptionHandling/GlobalExceptionHandler.cs`.

