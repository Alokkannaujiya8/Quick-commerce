# Error Handling & Problem Details Specification

This document details the error handling architecture, exception hierarchy, RFC 7807 Problem Details serialization, and validation failure formats across the QuickCart backend.

---

## 1. Overview & Protocol

QuickCart enforces **RFC 7807 (Problem Details for HTTP APIs)** as the standard error representation protocol across all REST endpoints. Error responses return the MIME type:
```http
Content-Type: application/problem+json
```

---

## 2. Global Exception Handler Implementation

Global exception processing is managed via ASP.NET Core's built-in `IExceptionHandler` abstraction, implemented by [`GlobalExceptionHandler`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/BuildingBlocks/BuildingBlocks.Presentation/ExceptionHandling/GlobalExceptionHandler.cs) in `BuildingBlocks.Presentation`.

### Registration Pipeline
1. **Service Registration** in [`DependencyInjection.cs`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/QuickCart.Api/Bootstrap/DependencyInjection.cs):
   ```csharp
   services.AddExceptionHandler<GlobalExceptionHandler>();
   services.AddProblemDetails();
   ```
2. **Middleware Activation** in [`MiddlewarePipeline.cs`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/QuickCart.Api/Bootstrap/MiddlewarePipeline.cs):
   ```csharp
   app.UseExceptionHandler();
   ```

---

## 3. Exception Hierarchy & Status Code Mappings

The handler converts specific domain and application exceptions into standardized RFC 7807 envelopes:

| Exception Type | HTTP Status Code | RFC Type URI | Problem Details Title |
| :--- | :--- | :--- | :--- |
| **`ValidationException`** | `400 Bad Request` | `https://tools.ietf.org/html/rfc7231#section-6.5.1` | `Validation Error` |
| **`NotFoundException`** | `404 Not Found` | `https://tools.ietf.org/html/rfc7231#section-6.5.4` | `Resource Not Found` |
| **`ConflictException`** | `409 Conflict` | `https://tools.ietf.org/html/rfc7231#section-6.5.8` | `Conflict` |
| **`UnauthorizedAccessException`** | `401 Unauthorized` | `https://tools.ietf.org/html/rfc7235#section-3.1` | `Unauthorized` |
| **Default / Unhandled `Exception`** | `500 Internal Server Error` | `https://tools.ietf.org/html/rfc7231#section-6.6.1` | `Internal Server Error` |

---

## 4. RFC 7807 Payload Examples

### 4.1 Validation Error (HTTP 400)
When input parameters fail validation, field-level failures are populated into the `errors` extension dictionary:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation Error",
  "status": 400,
  "detail": "One or more validation failures have occurred.",
  "instance": "/api/orders/checkout",
  "errors": {
    "DeliveryAddressId": [
      "DeliveryAddressId is required."
    ],
    "PaymentMethod": [
      "PaymentMethod 'Bitcoin' is not supported."
    ]
  }
}
```

### 4.2 Resource Not Found (HTTP 404)
When a queried entity does not exist:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Resource Not Found",
  "status": 404,
  "detail": "Order with identifier '5fa23d11-6548-43bb-81d3-f0a51e60472e' was not found.",
  "instance": "/api/orders/5fa23d11-6548-43bb-81d3-f0a51e60472e"
}
```

### 4.3 Conflict (HTTP 409)
When an action conflicts with the current business state:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.8",
  "title": "Conflict",
  "status": 409,
  "detail": "Order '5fa23d11-6548-43bb-81d3-f0a51e60472e' has already been paid.",
  "instance": "/api/payments/intents"
}
```

### 4.4 Unauthorized Access (HTTP 401)
When access is rejected due to invalid credentials, expired tokens, or IDOR violations:
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Order does not belong to the current user.",
  "instance": "/api/payments/intents"
}
```

---

## 5. Result Pattern (`BuildingBlocks.Application.Results`)

In addition to exception-based handling, `BuildingBlocks.Application.Results` provides a functional `Result` and `Result<TValue>` pattern:
* **`Result.Success()`** / **`Result.Success(value)`**: Indicates successful execution.
* **`Result.Failure(error)`**: Encapsulates an `Error` record with `Code` and `Message`.
* **Standard Errors**:
  * `Error.NullValue`
  * `Error.NotFound(entity, key)`
  * `Error.Conflict(message)`
  * `Error.Validation(code, message)`

---

## 6. Correlation IDs & Request Tracing

* **Status**: `Status: Not Implemented / Not Confirmed`
* **Evidence**:
  * Inspected [`GlobalExceptionHandler.cs`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/BuildingBlocks/BuildingBlocks.Presentation/ExceptionHandling/GlobalExceptionHandler.cs), [`MiddlewarePipeline.cs`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/QuickCart.Api/Bootstrap/MiddlewarePipeline.cs), and all module controllers.
  * Neither `HttpContext.TraceIdentifier` nor custom correlation headers (such as `X-Correlation-ID`) are currently attached to `ProblemDetails.Extensions` or propagated into structured logging scopes.

