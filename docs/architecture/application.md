# Application Layer Architecture

This document details the design, abstractions, interfaces, and patterns governing the application layer across the QuickCart modular monolith.

---

## 1. Role & Architectural Boundaries

The application layer (`<Module>.Application`) encapsulates the use cases, orchestration workflows, command/query contracts, and data transfer objects (DTOs) of each business module.

### Inward Dependency Rule
```text
[ Domain Layer ] <── [ Application Layer ] ──> [ BuildingBlocks.Application ]
```
* **Strict Independence**: The application layer has **zero** knowledge of persistence frameworks (EF Core), database connections, or HTTP serialization.
* **Service Contracts**: Defines interfaces for services, repositories, and third-party adapters that are implemented by `<Module>.Infrastructure`.

---

## 2. Shared Application Primitives (`BuildingBlocks.Application`)

### 2.1 CQRS Abstractions
[`CqrsAbstractions.cs`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/BuildingBlocks/BuildingBlocks.Application/CQRS/CqrsAbstractions.cs) defines the contracts for command-query separation:
* `ICommand<TResponse>` / `ICommand`: Represents intent to mutate system state.
* `IQuery<TResponse>`: Represents intent to read system state without side-effects.
* `ICommandHandler<TCommand, TResponse>`: Execution contract for commands.
* `IQueryHandler<TQuery, TResponse>`: Execution contract for queries.

### 2.2 Functional Result Pattern
[`Result.cs`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/BuildingBlocks/BuildingBlocks.Application/Results/Result.cs) provides a railway-oriented programming model for operations that can fail without throwing exceptions:
* `Result`: Represents a non-value operation outcome (`IsSuccess`, `IsFailure`, `Error`).
* `Result<TValue>`: Carries a typed payload on success or an `Error` on failure.
* `Error`: Immutable record containing `Code` and `Message`. Factory methods provide standard error definitions:
  * `Error.NotFound(entity, key)`
  * `Error.Conflict(message)`
  * `Error.Validation(code, message)`
  * `Error.NullValue`

### 2.3 Pagination Primitives
[`PagedResult.cs`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/BuildingBlocks/BuildingBlocks.Application/Pagination/PagedResult.cs) models standardized paged query collections:
* `Items`: Collection of typed elements (`IReadOnlyList<T>`).
* `TotalCount`: Total matching records.
* `Page` & `PageSize`: Pagination bounds.
* `TotalPages`: Dynamically computed `(int)Math.Ceiling(TotalCount / (double)PageSize)`.
* `HasPreviousPage` & `HasNextPage`: Navigation booleans.

### 2.4 Application Exceptions
[`Exceptions.cs`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/BuildingBlocks/BuildingBlocks.Application/Exceptions/Exceptions.cs) defines domain-aware application exceptions translated by `GlobalExceptionHandler`:
* `NotFoundException`: Thrown when a queried entity does not exist.
* `ValidationException`: Holds a dictionary of property-level validation failure messages.
* `ConflictException`: Thrown when an operation contradicts business state.

---

## 3. Module Application Service Interfaces

Each domain module exposes strongly-typed contracts under its `<Module>.Application` assembly:

| Module | Core Application Interfaces | Primary DTOs |
| :--- | :--- | :--- |
| **Catalog** | `ICatalogService` | `ProductDto`, `CategoryDto`, `SubCategoryDto`, `BrandDto` |
| **Inventory** | `IInventoryService`, `IGeoLocationService` | `DarkStoreDto`, `StoreInventoryDto`, `ServiceabilityResponse` |
| **Ordering** | `IOrderService` | `OrderDto`, `OrderItemDto`, `OrderStatusHistoryDto`, `CheckoutRequest` |
| **Delivery** | `IDeliveryService`, `IETACalculationService` | `DeliveryPartnerDto`, `DeliveryTrackingDto`, `AssignmentResult` |
| **Identity** | `IIdentityService`, `IJwtTokenService`, `IGoogleTokenValidator`, `IUserRepository` | `AuthResponse`, `TokenResponse`, `GoogleUserPayload`, `UserDto` |
| **Payment** | `IPaymentService`, `IPaymentGatewayService`, `IPaymentRepository` | `PaymentDto`, `PaymentIntentRequest`, `VerifyPaymentRequest`, `RefundRequest` |
| **Cart** | *Managed via CartService contracts* | `CartDto`, `CartItemDto`, `AddToCartRequest` |
| **Promotion** | *Pending Application service contracts* | *Coupons & Offers DTOs* |

---

## 4. Cross-Module Contract Invocations

When one module requires services from another domain, it consumes the public application interface rather than accessing internal infrastructure:
* **Payment $\longrightarrow$ Ordering**: `PaymentService` injects `Ordering.Application.Services.IOrderService` to retrieve authoritative order amounts (`GetOrderByIdAsync`) and transition order states (`UpdateOrderStatusAsync`).

