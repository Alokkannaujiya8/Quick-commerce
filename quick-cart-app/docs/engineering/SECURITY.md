# QuickCart Frontend — Security Architecture & Guidelines

This document details the client-side security architecture, authentication lifecycle, token handling, route guards, Google Identity Services integration, and browser storage boundaries of the QuickCart frontend.

---

## 1. Authentication Architecture

The application supports three verified authentication channels, unified under `AuthService` (`src/app/core/auth/auth-service.ts`):

1. **OTP-Based Mobile Authentication**:
   * Client calls `/api/auth/otp/send` with `{ phoneNumber }`.
   * Server issues an OTP (logged to console in development).
   * Client calls `/api/auth/otp/verify` with `{ phoneNumber, code }`.
2. **Email & Password Authentication**:
   * Client calls `/api/auth/login` with `{ login, password, deviceName }`.
   * Passwords are submitted over HTTPS.
3. **Google Identity Services (GIS) Sign-In**:
   * Integrated via `GoogleIdentityService` using the official GIS library (`https://accounts.google.com/gsi/client`).
   * The client renders or invokes the GIS popup flow, receives a signed Google ID token credential (`JWT`), and sends it to `/api/auth/google`.
   * The backend validates the ID token against Google's public keys and returns internal QuickCart JWT credentials.

---

## 2. Token Management & Storage Boundaries

### Browser Storage Model
Tokens and profiles are stored in browser `localStorage`:
* `qc_access_token` / `accessToken`: JWT Bearer access token used to authenticate API requests.
* `qc_refresh_token` / `refreshToken`: Refresh token used to obtain a new access token upon expiration.
* `qc_user_profile`: Serialized JSON representation of the authenticated user's profile.

> [!WARNING]
> **Browser Storage Security Notice**:
> In accordance with standard browser security principles, data stored in `localStorage` is accessible to client-side scripts running on the same origin. It is therefore vulnerable if an XSS vulnerability exists. In production, sensitive token storage should ideally migrate to HttpOnly, SameSite, Secure cookies for refresh tokens. Currently, tokens are stored in `localStorage` as verified in `AuthService`.

### Token Refresh & Rotation Flow
* When an API call returns `401 Unauthorized`, `authInterceptor` catches the response.
* If a refresh token is present, it invokes `POST /api/auth/refresh` with `{ refreshToken, deviceName: 'Web Browser' }`.
* Upon success, the new tokens are written to `localStorage` and the pending request is retried.
* Upon failure, `clearSession()` purges all tokens and profile state from `localStorage` and resets `currentUser` to `null`.

### Logout Flow
* `logout()` sends a revocation request to `POST /api/auth/logout` with `{ refreshToken }` to invalidate the refresh token on the server.
* Invokes `clearSession()` to purge local storage keys.
* Navigates the user to `/login`.

---

## 3. Request Security & Interceptors (`authInterceptor`)

* **Automatic Header Injection**: Every outgoing HTTP request executed via Angular `HttpClient` passes through `authInterceptor` (`src/app/core/interceptors/auth.interceptor.ts`).
* If `authService.getAccessToken()` returns a valid token string, the interceptor clones the request and sets:
  ```text
  Authorization: Bearer <accessToken>
  ```
* Requests without a token proceed without the header (e.g. public catalog requests).

---

## 4. Route Protection (`authGuard`)

* Implemented as a modern functional route guard (`CanActivateFn` in `src/app/core/guards/auth.guard.ts`):
  ```typescript
  export const authGuard: CanActivateFn = () => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (authService.isAuthenticated()) {
      return true;
    }

    return router.createUrlTree(['/login']);
  };
  ```
* Evaluates `authService.isAuthenticated()`, which checks whether `currentUser()` or an access token is present.
* Redirects unauthenticated attempts to `/login` via `UrlTree`.

> [!NOTE]
> **Current Guard Route Attachment Status**:
> While `authGuard` is fully implemented and unit-tested, in the current `app.routes.ts`, it is not yet attached to the `/checkout` route. Users can currently browse the checkout page without prior authentication.

---

## 5. Payment Security Boundaries

* **No Card Data Handled by Frontend**:
  * The frontend never captures, processes, or stores primary credit/debit card numbers (PANs) or CVVs.
  * Payment initiation creates a server-side payment intent via `POST /api/payments/intents` containing only `{ orderId, paymentMethod, idempotencyKey }`.
* **Server-Authoritative Amounts**:
  * Total amounts and line-item prices are recalculated and verified authoritatively by the backend ordering and payment modules.
* **Idempotency Keys**:
  * Checkout requests generate client-side idempotency keys (e.g. `idem-${order.id}-${this.selectedPayment}`) to prevent accidental duplicate charges.
* **Signature Verification**:
  * Online payments verify cryptographic signatures via `POST /api/payments/verify` before updating the order status to `Captured`.

---

## 6. XSS & Template Sanitization

* **Angular Built-in Sanitization**: Angular automatically sanitizes values bound via `{{ ... }}` and `[property]` bindings to prevent Cross-Site Scripting (XSS).
* **No `bypassSecurityTrust*`**: The codebase does not use `DomSanitizer.bypassSecurityTrustHtml` or unsafe inner HTML injection.
* **No `eval()` or Unsafe Scripts**: All application scripts are bundled through Angular's build pipeline with strict TypeScript compile settings.
