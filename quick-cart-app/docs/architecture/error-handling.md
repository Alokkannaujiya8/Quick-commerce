# Error Handling Architectural Specification

This document details the architectural error-handling layers, error models, and resiliency fallbacks designed into the QuickCart frontend.

---

## 1. Architectural Strategy

The QuickCart error handling strategy is built on three tiers:
1. **Infrastructure Tier (`authInterceptor`, `ApiService`)**: Catches and standardizes network errors, handles transparent authentication retries, and logs unhandled exceptions.
2. **Domain Service Tier (`CatalogService`, `OrderService`, `PaymentService`)**: Transforms backend errors, prevents application crashes, and provides offline fallback mocks when local development backends are unavailable.
3. **View Tier (Components & Presenters)**: Translates RFC 7807 Problem Details into customer-friendly notifications rendered via `AlertBannerComponent` or inline form helpers.

---

## 2. Problem Details (RFC 7807) Integration

When the QuickCart backend rejects a request due to validation errors, business rule conflicts, or domain exceptions, it responds with an RFC 7807 Problem Details object:
```json
{
  "type": "https://quickcart.com/errors/conflict",
  "title": "Conflict",
  "status": 409,
  "detail": "A user with this phone number already exists.",
  "instance": "/api/auth/register"
}
```

### Component Extraction Pattern
Across all auth and order pages, error parsing inspects the payload in descending order of specificity:
```typescript
const msg =
  err?.error?.detail ||
  err?.error?.message ||
  (err?.status === 401
    ? 'Authentication failed. Please verify credentials.'
    : 'An unexpected issue occurred. Please try again.');
this.errorMessage.set(msg);
```

---

## 3. Resiliency & Offline Fallback Architecture

To enable smooth offline product demonstrations and independent frontend UI development without requiring a live PostgreSQL or .NET backend instance, domain services implement intelligent fallbacks:

### 3.1 Catalog Resiliency (`CatalogService`)
* If `GET /api/catalog/categories` fails, returns 6 default categories (`Fruits & Vegetables`, `Dairy, Bread & Eggs`, etc.).
* If `GET /api/catalog/products` fails, returns 8 default in-memory grocery products with images, prices, and unit measurements.

### 3.2 Ordering Resiliency (`OrderService`)
* If `POST /api/orders/checkout` fails (e.g. backend offline), catches error with `catchError`, logs a warning to the console, generates a mock confirmed order (`generateMockOrder`), stores it in `localStorage`, and permits the user to proceed to the order success page.

### 3.3 Payment Step Resiliency (`CheckoutComponent`)
* If `POST /api/payments/intents` or `/api/payments/verify` fails during local development without the payment gateway configured, catches the error and gracefully completes the order workflow.
