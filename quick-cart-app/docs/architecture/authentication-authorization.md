# Authentication & Authorization Architecture

This document details the identity workflows, multi-channel authentication methods, token lifecycle, Google Identity Services integration, and role authorization status in the QuickCart frontend.

---

## 1. Authentication Modalities

The QuickCart frontend implements three distinct customer authentication flows, managed by `AuthService` (`src/app/core/auth/auth-service.ts`):

```text
               ┌──────────────────────────────┐
               │    Customer Entry (/login)   │
               └──────────────┬───────────────┘
                              │
         ┌────────────────────┼────────────────────┐
         ▼                    ▼                    ▼
  [ Mobile OTP ]      [ Email/Password ]     [ Google GIS ]
         │                    │                    │
         ▼                    ▼                    ▼
/api/auth/otp/send    /api/auth/login       /api/auth/google
         │                    │                    │
/api/auth/otp/verify          │                    │
         │                    │                    │
         └────────────────────┼────────────────────┘
                              │
                              ▼
                     [ AuthResponse DTO ]
                    Tokens + User Profile
                              │
                              ▼
             localStorage (qc_access_token, ...)
                              │
                              ▼
                Auth Signals Updated (currentUser)
```

### 1.1 Phone OTP Authentication
1. **Initiation**: Customer submits a 10-digit mobile number in `LoginComponent`.
2. **API Call**: `AuthService.sendOtp(phoneNumber)` posts to `POST /api/auth/otp/send`.
3. **Redirection**: On success, the application stores `pendingPhoneNumber` and navigates to `/login/verify?phone=<number>`.
4. **Verification**: In `OtpVerifyComponent`, the customer enters a 6-digit OTP code and submits `AuthService.verifyOtp(phone, code)` via `POST /api/auth/otp/verify`.
5. **Session**: On successful verification, the response tokens are stored and the user is redirected to `/`.

### 1.2 Password Authentication
1. Customer clicks "Use Email & Password" in `LoginComponent` and navigates to `/login/password`.
2. In `PasswordLoginComponent`, the customer provides email and password.
3. Invokes `AuthService.login({ login, password, deviceName: 'Web Browser' })` via `POST /api/auth/login`.
4. On success, writes tokens and navigates to `/`.

### 1.3 Google Identity Services (GIS) Sign-In
1. Handled by `GoogleIdentityService` (`src/app/core/auth/google-identity.service.ts`).
2. Script dynamically loaded once from `https://accounts.google.com/gsi/client`.
3. Client initializes GIS with `client_id` from `environment.googleClientId`.
4. User clicks "Continue with Google" -> triggers popup -> Google emits ID token credential.
5. Emitted ID token is posted to `POST /api/auth/google` by `AuthService.googleSignIn(idToken)`.
6. Server validates Google token and returns QuickCart session tokens.

---

## 2. Session & Token Management

### Storage Keys
* `qc_access_token` / `accessToken`: JWT Bearer access token string.
* `qc_refresh_token` / `refreshToken`: Refresh token string for session renewal.
* `qc_user_profile`: JSON string representing the authenticated `User` object.

### Token Refresh Pipeline
When an API request returns `401 Unauthorized` on a non-auth endpoint, `authInterceptor` activates:
```typescript
authService.refreshToken().pipe(
  switchMap((authResponse) => {
    const newAccessToken = authResponse.tokens?.accessToken || authService.getAccessToken();
    const retriedRequest = newAccessToken
      ? req.clone({ setHeaders: { Authorization: `Bearer ${newAccessToken}` } })
      : req;
    return next(retriedRequest);
  })
)
```
If refresh succeeds, the pending request is completed seamlessly. If refresh fails, `clearSession()` is executed, wiping all local tokens and resetting `currentUser.set(null)`.

---

## 3. Authorization & Roles

* **Role Definition**: The `User` interface includes an optional `role?: string` field (`src/app/core/auth/auth.models.ts`).
* **Current Status**:
  ```text
  Status: Customer Authentication Active; Role-Based Access Control (RBAC) Not Implemented

  Evidence:
  Inspected src/app/core/guards/auth.guard.ts. The guard only checks `authService.isAuthenticated()` (boolean check for user existence). No role checks (e.g. Admin, DeliveryPartner, Customer) or permission guards exist in the frontend routes.
  ```
