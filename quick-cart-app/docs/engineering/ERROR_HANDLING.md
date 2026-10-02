# QuickCart Frontend — Error Handling Specification

This document details the actual error handling patterns, HTTP status code management, ProblemDetails parsing, fallback resilience, and user-facing error state implementations across the QuickCart frontend.

---

## 1. Architectural Error Layers

```text
Backend RFC 7807 Error (HTTP 4xx / 5xx)
            │
            ▼
Angular HttpClient (HttpErrorResponse)
            │
            ▼
authInterceptor (Catches 401, checks exclusions, executes token refresh or forwards)
            │
            ▼
ApiService (Logs to console.error, forwards via catchError(this.handleError))
            │
            ▼
Domain Services (Transforms error, activates fallback mock data if applicable)
            │
            ▼
Component (Extracts detail/message into signal, displays via AlertBanner / UI)
```

---

## 2. HTTP Error Response & Problem Details Handling

The backend API returns errors adhering to RFC 7807 **Problem Details** (`application/problem+json`):
```json
{
  "type": "https://quickcart.com/errors/validation",
  "title": "Validation Failed",
  "status": 400,
  "detail": "One or more fields failed validation.",
  "errors": {
    "Quantity": ["Quantity must be greater than 0."]
  },
  "instance": "/api/cart/items"
}
```

### Client-Side Extraction Pattern
Across feature components (`LoginComponent`, `RegisterComponent`, `OtpVerifyComponent`, `PasswordLoginComponent`), error messages are parsed hierarchically:
```typescript
error: (err: any) => {
  this.isLoading.set(false);
  this.isErrorToast.set(true);
  const msg =
    err?.error?.detail ||
    err?.error?.message ||
    'An unexpected error occurred. Please try again.';
  this.toastMessage.set(msg);
}
```
* **Primary Key**: `err?.error?.detail` extracts the RFC 7807 problem detail message.
* **Secondary Key**: `err?.error?.message` extracts generic message fields.
* **Fallback**: User-friendly static fallback string.

---

## 3. Interceptor Error Behavior (`authInterceptor`)

The functional HTTP interceptor `src/app/core/interceptors/auth.interceptor.ts` intercepts all outgoing requests and catches errors via `catchError`:

1. **Authentication Endpoint Exclusions**:
   Requests to authentication endpoints are excluded from the 401 retry loop to prevent infinite recursive calls:
   * `/auth/login`
   * `/auth/register`
   * `/auth/google`
   * `/auth/otp/`
   * `/auth/refresh`
2. **Automatic Token Refresh on 401**:
   When an unauthorized (`401`) error is received on a protected domain endpoint and a refresh token exists in storage:
   * Calls `authService.refreshToken()`.
   * Upon successful refresh, clones the failed request with the new `Bearer <newAccessToken>` and retries the request via `next(retriedRequest)`.
   * If the refresh call itself fails, `authService.clearSession()` purges local tokens and rethrows the error, forcing the user to log in again.
3. **Non-401 Errors**:
   All other errors (`400`, `403`, `404`, `409`, `500`) pass directly through to the calling service.

---

## 4. Feature-Specific Error Flows

### 4.1 Authentication Failures
* **OTP Verification Failure**:
  * Emits error from `/api/auth/otp/verify`.
  * Catches error in `OtpVerifyComponent.onVerify()`.
  * Renders error: `"Invalid or expired OTP code. Please check the console log and try again."`
* **Google Authentication Failure**:
  * If Google client ID is unconfigured: emits descriptive error message.
  * If token verification fails (HTTP 401): renders `"Google account could not be verified."`
  * Cleans up loading state signal: `isGoogleLoading.set(false)`.

### 4.2 Checkout & Payment Errors
* **Order Creation Error**:
  * If `/api/orders/checkout` fails (e.g. backend unreachable in offline demo), `OrderService.createOrder()` falls back via `catchError`:
    ```typescript
    catchError((err) => {
      console.warn('Backend orders/checkout unreachable, falling back to local order state:', err);
      const mockOrder = this.generateMockOrder(request);
      this.saveOrder(mockOrder);
      return of(mockOrder);
    })
    ```
* **Payment Step Fallback**:
  * In `CheckoutComponent`, if payment intent or verification fails:
    ```typescript
    catchError((err) => {
      console.warn('Payment API step fallback:', err);
      return of(order);
    })
    ```
  * Ensures that offline checkout flows do not strand the customer with an unhandled exception.

### 4.3 Catalog Fallbacks
* If `/api/catalog/categories` or `/api/catalog/products` fails:
  * `CatalogService` intercepts the error and returns in-memory mock datasets (`fallbackCategories`, `fallbackProducts`).
  * Prevents empty white screens when the API backend is offline.

---

## 5. User-Facing Error UI

* **Alert Banners**: Implemented via `AlertBannerComponent` (`src/app/shared/components/alert-banner/`).
  * Supports `variant="danger"` and `variant="success"`.
  * Supports dismissible action (`(close)="toastMessage.set(null)"`).
* **Signal-Driven Banners**: Component signals (`errorMessage`, `toastMessage`, `isErrorToast`) dynamically show and hide error alerts using `@if (errorMessage()) { ... }`.
