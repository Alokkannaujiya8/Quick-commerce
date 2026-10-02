# Engineering Memory & Invariants

This document captures enduring architectural knowledge, design invariants, non-negotiable constraints, and repository conventions for the QuickCart platform.

---

## 1. Prime Invariants (Do Not Break)

1. **Zero Cross-Schema Foreign Keys**:
   * Entities in one schema (e.g. `ordering.Orders`) must never declare physical database foreign keys or EF Core navigation properties to entities in another schema (e.g. `identity.Users` or `catalog.Products`).
   * References must be stored as immutable `Guid` values (`UserId`, `ProductId`, `DeliveryAddressId`).
2. **Authoritative Server Pricing**:
   * Total amounts, subtotals, and delivery fees must never be trusted from client HTTP payloads.
   * `PaymentService` queries `IOrderService.GetOrderByIdAsync` to obtain the authoritative database amount before creating payment intents.
3. **No Database Dependencies in Domain or Application**:
   * `<Module>.Domain` and `<Module>.Application` must never reference `Microsoft.EntityFrameworkCore` or data access libraries.
4. **Pure UTC Timestamps**:
   * All database and API timestamps are in UTC (`DateTime.UtcNow`). `AuditableEntitySaveChangesInterceptor` automates `CreatedAt` and `UpdatedAt` auditing.
5. **Constant-Time Cryptographic Verification**:
   * Webhook signatures and payment verification hashes must always be compared using `CryptographicOperations.FixedTimeEquals` to prevent side-channel timing attacks.
6. **Hashed Refresh Tokens**:
   * Plain text refresh tokens are never persisted. Only the SHA-256 digest (`TokenHash`) is stored in `identity.RefreshTokens`.
7. **Frontend Isolation**:
   * Do not touch files in `quick-cart-app/` during backend tasks unless explicitly asked.

---

## 2. Cross-Module Communication Pattern

* **In-Process Application Contract Invocation**:
  * Allowed: Referencing `<OtherModule>.Application` from `<Module>.Infrastructure` or `<Module>.Application`.
  * Example: `Payment.Infrastructure` references `Ordering.Application.Services.IOrderService`.
* **Prohibited**:
  * Direct cross-referencing between `<ModuleA>.Infrastructure` and `<ModuleB>.Infrastructure`.
  * Injecting another module's `DbContext` (e.g., injecting `OrderingDbContext` into `PaymentService` is strictly forbidden).

---

## 3. Database Schema Mapping Rules

Each module maps to its isolated PostgreSQL schema:
* `catalog` $\rightarrow$ `CatalogDbContext`
* `inventory` $\rightarrow$ `InventoryDbContext`
* `ordering` $\rightarrow$ `OrderingDbContext`
* `cart` $\rightarrow$ `CartDbContext`
* `delivery` $\rightarrow$ `DeliveryDbContext`
* `identity` $\rightarrow$ `IdentityDbContext`
* `payment` $\rightarrow$ `PaymentDbContext`
* `promotion` $\rightarrow$ `PromotionDbContext`

When adding a new migration:
* Always specify the `--context <ModuleName>DbContext` flag.
* Store migrations under `<ModuleName>.Infrastructure/Persistence/Migrations`.

---

## 4. Error Handling Protocol

* Throw domain exceptions from `BuildingBlocks.Application.Exceptions`:
  * `NotFoundException` $\rightarrow$ HTTP 404
  * `ValidationException` $\rightarrow$ HTTP 400
  * `ConflictException` $\rightarrow$ HTTP 409
  * `UnauthorizedAccessException` $\rightarrow$ HTTP 401
* Let `GlobalExceptionHandler` format the response into standard RFC 7807 Problem Details (`application/problem+json`).
* Do not return naked HTTP status codes or catch exceptions without rethrowing or translating them.

---

## 5. Build & Test Verification Commands

* **Build**:
  ```powershell
  dotnet build QuickCart.sln
  ```
* **Test**:
  ```powershell
  dotnet test QuickCart.sln
  ```
* **Apply Migrations**:
  ```powershell
  ./scripts/migrate-database.ps1
  ```

