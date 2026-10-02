# Observability, Logging & Auditability Specification

This document details the logging framework, audit trail persistence, health check endpoints, and telemetry capabilities across the QuickCart backend.

---

## 1. Structured Logging

* **Framework**: Built-in `Microsoft.Extensions.Logging` integrated via ASP.NET Core dependency injection.
* **Configuration**: Configured in `appsettings.json` and `appsettings.Development.json`:
  ```json
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Identity": "Information"
    }
  }
  ```
* **Injection & Conventions**: Classes inject `ILogger<T>` and use semantic message templates:
  ```csharp
  _logger.LogInformation("Created payment intent {PaymentId} for Order {OrderId}", payment.Id, payment.OrderId);
  ```

---

## 2. Persistent Business Audit Trails

QuickCart captures explicit, immutable audit logs for critical domain state transitions:

### 2.1 Payment Audit Trail (`payment.PaymentAuditLogs`)
Every payment lifecycle change generates an audit record:
* `PaymentId` (`uuid`, FK to `payment.Payments`)
* `EventType` (`varchar(60)`): E.g. `PaymentInitiated`, `PaymentSignatureVerified`, `PaymentFailed`, `PaymentRefunded`.
* `PreviousStatus` (`varchar(30)`): State before transition.
* `NewStatus` (`varchar(30)`): Resulting state.
* `ProviderReference` (`varchar(128)`): Gateway transaction reference.
* `Notes` (`varchar(500)`): Diagnostic details.
* `CreatedAt` (`timestamptz`): UTC timestamp.

### 2.2 Order Status Audit Trail (`ordering.OrderStatusHistories`)
Every progression through the order fulfillment pipeline is recorded:
* `OrderId` (`uuid`, FK to `ordering.Orders`)
* `Status` (`varchar(50)`): `Placed`, `Confirmed`, `Packed`, `OutForDelivery`, `Delivered`, `Cancelled`.
* `ChangedAt` (`timestamptz`): UTC timestamp.
* `Notes` (`varchar(500)`): Transition triggers (e.g. "Order confirmed for Cash on Delivery", "Payment captured").

### 2.3 Inventory Movement Ledger (`inventory.StockMovements`)
Records historical adjustments and movements of stock items per dark store.

### 2.4 Entity Change Auditing (`AuditableEntitySaveChangesInterceptor`)
Any entity implementing `IAuditableEntity` automatically has its timestamps stamped in UTC upon saving changes to PostgreSQL:
* `CreatedAt = DateTime.UtcNow` (on `EntityState.Added`)
* `UpdatedAt = DateTime.UtcNow` (on `EntityState.Modified`)

---

## 3. Health Checks

A built-in health check endpoint is registered in [`EndpointRegistration.cs`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/QuickCart.Api/Bootstrap/EndpointRegistration.cs):
* **Endpoint**: `GET /health`
* **Response**: `200 OK`
  ```json
  {
    "status": "Healthy",
    "service": "QuickCart.Api",
    "timestamp": "2026-10-02T06:20:00.0000000Z"
  }
  ```

---

## 4. Unimplemented Observability Features

### 4.1 Correlation / Request IDs
* **Status**: `Status: Not Implemented / Not Confirmed`
* **Evidence**: Inspected [`GlobalExceptionHandler.cs`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/BuildingBlocks/BuildingBlocks.Presentation/ExceptionHandling/GlobalExceptionHandler.cs), [`MiddlewarePipeline.cs`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/QuickCart.Api/Bootstrap/MiddlewarePipeline.cs), and controller actions. No correlation middleware or `HttpContext.TraceIdentifier` headers are attached to responses or log scopes.

### 4.2 Distributed Tracing & Metrics (OpenTelemetry / Prometheus)
* **Status**: `Status: Not Implemented / Not Confirmed`
* **Evidence**: Inspected all `.csproj` files and `Program.cs`. No OpenTelemetry SDKs, Prometheus metric exporters, or custom `ActivitySource` instruments are present in the backend.

### 4.3 Background Job Telemetry
* **Status**: `Status: Not Implemented / Not Confirmed`
* **Evidence**: No background workers are currently implemented.

