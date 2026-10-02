# HTTP Interceptors Architecture Specification

This document details the functional HTTP interceptor architecture, header injection mechanics, and 401 unauthorized refresh token rotation in the QuickCart frontend.

---

## 1. Functional Interceptor Pattern

Angular 20 employs **functional interceptors** (`HttpInterceptorFn`) rather than class-based `HttpInterceptor` providers.

The interceptor is registered in `src/app/app.config.ts`:
```typescript
export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(withInterceptors([authInterceptor])),
    // ...
  ],
};
```

---

## 2. `authInterceptor` Implementation Details

The interceptor is located at `src/app/core/interceptors/auth.interceptor.ts`.

### 2.1 Request Header Mutation
For every outgoing HTTP request:
1. Injects `AuthService` using `inject(AuthService)`.
2. Inspects `authService.getAccessToken()` (read from `localStorage` key `qc_access_token`).
3. If a non-empty token exists:
   * Clones the request and adds `Authorization: Bearer <token>`.
4. If no token exists:
   * Passes the original request untouched (`req`).

```typescript
const token = authService.getAccessToken();

const authRequest = token
  ? req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`,
      },
    })
  : req;
```

---

## 3. Error Interception & Token Refresh Rotation

The interceptor pipes through `catchError` to handle failure responses:

```text
[ Outgoing Request ] ──────> [ Backend Returns HTTP 401 ]
                                         │
                   ┌─────────────────────┴─────────────────────┐
                   ▼                                           ▼
      Is Endpoint in Exclusions?                     Is Protected Domain Endpoint?
                   │                                           │
                  YES                                         YES
                   │                                           │
                   ▼                                           ▼
          Throw Error Immediately                     Has qc_refresh_token?
                                                               │
                                             ┌─────────────────┴─────────────────┐
                                             ▼                                   ▼
                                            YES                                  NO
                                             │                                   │
                                             ▼                                   ▼
                                authService.refreshToken()               Throw Error (401)
                                             │
                             ┌───────────────┴───────────────┐
                             ▼                               ▼
                          SUCCESS                         FAILURE
                             │                               │
                             ▼                               ▼
                 Clone request with new token         clearSession()
                     Retry original request         Throw Refresh Error
```

### 3.1 Endpoint Exclusions
To prevent infinite recursive loops when credentials fail on authentication endpoints, the following URLs bypass the refresh attempt:
* `/auth/login`
* `/auth/register`
* `/auth/google`
* `/auth/otp/`
* `/auth/refresh`

### 3.2 Request Retry Mechanics
Upon receiving a new token from `authService.refreshToken()`, the interceptor executes:
```typescript
return authService.refreshToken().pipe(
  switchMap((authResponse) => {
    const newAccessToken = authResponse.tokens?.accessToken || authService.getAccessToken();
    const retriedRequest = newAccessToken
      ? req.clone({
          setHeaders: {
            Authorization: `Bearer ${newAccessToken}`,
          },
        })
      : req;
    return next(retriedRequest);
  })
);
```
The failed request is seamlessly retried with the renewed Bearer token, completing the user's operation without interruption.
