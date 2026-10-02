# Engineering Rules & Guidelines

This document outlines the mandatory engineering rules, coding standards, architectural principles, and safety guidelines enforced across the QuickCart backend codebase.

---

## 1. Core Architectural Principles

### 1.1 Modular Monolith Isolation
* **Autonomous Boundaries**: Each domain capability is an isolated module located in `src/Modules/<ModuleName>/`.
* **Zero Cross-Schema Joins**: Database queries must never perform cross-schema SQL joins or configure cross-schema navigation properties.
* **Identity References**: Cross-module entity references must strictly be logical `Guid` values (e.g. `UserId`, `OrderId`, `ProductId`).
* **Inter-Module Contract Invocations**: Modules communicate in-process by referencing and consuming another module's public Application contracts (e.g. `Payment.Infrastructure` invokes `Ordering.Application.Services.IOrderService`). Direct DbContext cross-referencing is strictly forbidden.

### 1.2 Clean Architecture (Inward Dependency)
Within each module, projects must strictly adhere to inward Clean Architecture dependencies:
```text
[ Domain ] <── [ Application ] <── [ Infrastructure ]
                               <── [ Presentation ]
```
* **Domain Layer (`<Module>.Domain`)**:
  * Depends only on `BuildingBlocks.Domain`.
  * Must contain zero external NuGet packages, EF Core attributes, or HTTP dependencies.
  * Invariants, business validations, and domain events belong exclusively here.
* **Application Layer (`<Module>.Application`)**:
  * Depends only on `<Module>.Domain` and `BuildingBlocks.Application`.
  * Must never reference EF Core, database drivers, or ASP.NET Core presentation primitives.
  * Holds use case orchestration, DTOs, service interfaces, and CQRS contracts.
* **Infrastructure Layer (`<Module>.Infrastructure`)**:
  * Implements application interfaces, EF Core `DbContext`, entity type configurations, repositories, and external gateway adapters.
  * Must not be referenced by Domain or Application.
* **Presentation Layer (`<Module>.Presentation`)**:
  * Contains API controllers (`ControllerBase`), route models, and SignalR hubs.
  * Depends on Application, Domain, and `BuildingBlocks.Presentation`.
  * Controllers must never access `DbContext` or write direct queries; all logic flows through application services.

### 1.3 SOLID Principles
* **Single Responsibility (SRP)**: Each class must have one reason to change. Separate entity validation, database access, external gateway communication, and HTTP serialization.
* **Open/Closed (OCP)**: Extend behavior via abstractions (`IOrderService`, `IPaymentGatewayService`, `IGoogleTokenValidator`) rather than modifying established core workflows.
* **Liskov Substitution (LSP)**: Interface implementations must conform strictly to contract semantics without throwing unexpected exceptions.
* **Interface Segregation (ISP)**: Maintain focused, client-specific interfaces rather than monolithic "God interfaces".
* **Dependency Inversion (DIP)**: High-level modules and application services must depend on abstractions, not concrete implementations. Concrete dependencies are injected in `src/QuickCart.Api/Bootstrap/DependencyInjection.cs`.

---

## 2. C# & .NET 10 Coding Standards

### 2.1 Language & Type Safety
* **Language Version**: C# 14.0 on .NET 10 (`net10.0`), enforced via `Directory.Build.props`.
* **Nullable Reference Types**: `<Nullable>enable</Nullable>` is enabled solution-wide.
  * Never suppress null warnings with `!` unless proven unreachable and documented with a comment.
  * Explicitly annotate nullable parameters and properties with `?`.
* **Immutability & Records**: Use C# `record` types for DTOs, value objects, request models, and response envelopes.

### 2.2 Async/Await Rules
* **Asynchronous I/O**: Every database call, HTTP call, and external service call must be asynchronous (`async`/`await`).
* **No Blocking Calls**: Never use `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()`. Blocking asynchronous calls leads to thread starvation and deadlocks.
* **Cancellation Tokens**: Always accept a `CancellationToken cancellationToken = default` parameter in asynchronous methods and propagate it to EF Core and I/O methods.
* **ValueTask for Hot Paths**: Use `ValueTask` or `ValueTask<T>` where synchronous completion is common (such as interceptors).

### 2.3 Time & Date Handling
* **UTC Everywhere**: All timestamps stored in the database or transmitted over the wire must be in UTC (`DateTime.UtcNow`).
* **Automatic Auditing**: Implement `IAuditableEntity` on entities requiring timestamps. The `AuditableEntitySaveChangesInterceptor` populates `CreatedAt` on insertion and `UpdatedAt` on modification.

---

## 3. Validation & Exception Handling Rules

### 3.1 Validation Rules
* Validate request parameters and payloads early at the application or controller boundary.
* Do not rely solely on database constraints for business rule enforcement.
* Use `ValidationException` with field-level error dictionaries for parameter violations.

### 3.2 Exception Rules
* **Do Not Catch Without Cause**: Never catch generic `Exception` unless logging or translating into a well-defined domain failure.
* **Use Standard Domain Exceptions**:
  * `NotFoundException`: When an aggregate cannot be found for a given identifier (produces HTTP 404).
  * `ValidationException`: When input data violates structural or business validation rules (produces HTTP 400).
  * `ConflictException`: When an action cannot proceed due to the current state of a resource (produces HTTP 409).
  * `UnauthorizedAccessException`: When a caller is unauthenticated or attempting an IDOR violation (produces HTTP 401).
* **Global Handling**: All unhandled exceptions are caught by `GlobalExceptionHandler` and rendered in RFC 7807 Problem Details format (`application/problem+json`).

---

## 4. Logging & Observability Rules

* **Structured Logging**: Inject `ILogger<T>` into classes. Use named message templates with named placeholders (e.g. `_logger.LogInformation("Order {OrderId} created for User {UserId}", order.Id, order.UserId)`). Never use string concatenation (`$""`) in log messages.
* **No Sensitive Data in Logs**: Never log passwords, raw credit card details, JWT secrets, webhook signature secrets, or unhashed refresh tokens.
* **Audit Logs**: State transitions in high-stakes domains (such as order status updates and payment transactions) must write persistent audit rows to dedicated audit tables (`ordering.OrderStatusHistories`, `payment.PaymentAuditLogs`).

---

## 5. Database & Migration Rules

* **Single Source of Truth**: The database schema is defined and evolved exclusively through EF Core migrations.
* **Schema Prefixing**: Every module DbContext must explicitly declare its schema using `modelBuilder.HasDefaultSchema("<schema>")`.
* **Zero Manual SQL Migrations in Source Control**: Never hand-edit snapshot files or generated migration designer files.
* **Migration Command**: Always specify `--project`, `--startup-project`, `--context`, and `--output-dir` when running `dotnet ef migrations add`.

---

## 6. Git Safety Rules

* **Never run destructive commands**:
  * Prohibited: `git reset`, `git restore`, `git checkout .`, `git clean`, `git stash`, `git rebase`, `git commit`, `git push`.
* **Preserve User Changes**: Uncommitted user files must not be altered, reverted, or overwritten.
* **Inspect Git Status**: Always verify `git status` prior to and upon completing work.

