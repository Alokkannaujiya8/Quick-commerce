# ADR 0003: Functional JWT Interception & Silent 401 Refresh Rotation

## Status
**Accepted**

## Context
QuickCart API endpoints require JWT authentication via `Authorization: Bearer <token>`. Access tokens have a finite lifetime. When an access token expires during a user's session (e.g. during browsing or checkout), requests fail with `401 Unauthorized`. Forcing the customer to log in again repeatedly damages user experience in a fast-paced 10-minute grocery delivery app.

## Decision
Implement a functional Angular HTTP interceptor (`authInterceptor` in `src/app/core/interceptors/auth.interceptor.ts`) that:
1. Automatically attaches `Authorization: Bearer <accessToken>` to all outgoing requests if a token is present in storage.
2. Intercepts `401 Unauthorized` responses on domain endpoints (excluding login, register, and refresh endpoints).
3. Transparently calls `authService.refreshToken()` to acquire a new access token using the stored refresh token.
4. Retries the failed request with the newly issued token and completes the original observer stream.
5. Purges local session and redirects to `/login` if token renewal fails.

## Consequences
### Positive
* Transparent, frictionless user experience: sessions are renewed without user intervention or page reload.
* Centralized token injection removes authentication concern from individual domain services.
* Explicit exclusion list prevents infinite 401 recursion.

### Negative / Trade-offs
* Concurrent 401 requests could potentially trigger multiple refresh calls if not locked or queued.
