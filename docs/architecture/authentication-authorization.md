# Authentication & Authorization Architecture

This document details the identity architecture, token lifecycles, cryptographic operations, external identity provider flows, and endpoint authorization mechanisms across QuickCart.

---

## 1. Authentication Strategy: Dual-Token System

QuickCart uses a **stateless dual-token authentication pattern**:

```text
┌──────────────┐                                       ┌─────────────────────────┐
│              │ ── 1. POST /api/auth/login ─────────> │                         │
│              │ <── 2. JWT Access Token + Refresh ─── │                         │
│              │        Token (64-byte CSPRNG)         │                         │
│              │                                       │                         │
│ Angular SPA  │ ── 3. GET /api/orders (Bearer JWT) ─> │      QuickCart.Api      │
│   Client     │ <── 4. 200 OK (Orders Data) ───────── │                         │
│              │                                       │                         │
│              │ ── 5. POST /api/auth/refresh ───────> │  (Validates SHA-256     │
│              │ <── 6. Rotated Token Pair ─────────── │   hash & rotates token) │
└──────────────┘                                       └─────────────────────────┘
```

### 1.1 Access Token (JWT)
* **Lifetime**: 15 minutes (`Jwt:AccessTokenMinutes`).
* **Signature Algorithm**: HMAC-SHA256 (`SecurityAlgorithms.HmacSha256`).
* **Signing Key**: Sourced from configuration (`Jwt:SecretKey`). Must exceed 256 bits (32 bytes).
* **Claims Attached**:
  * `JwtRegisteredClaimNames.Sub`: User ID (`Guid`)
  * `JwtRegisteredClaimNames.Jti`: Unique Token ID (`Guid`)
  * `ClaimTypes.NameIdentifier`: User ID (`Guid`)
  * `ClaimTypes.Name`: User Full Name
  * `ClaimTypes.MobilePhone`: Verified phone number
  * `ClaimTypes.Email`: Verified email address

### 1.2 Refresh Token
* **Generation**: Generated using `System.Security.Cryptography.RandomNumberGenerator.GetBytes(64)`.
* **Lifetime**: 30 days (`Jwt:RefreshTokenDays`).
* **Storage Protection**: The plain-text refresh token is **never stored** in the database. Only its cryptographic SHA-256 digest (`TokenHash`) is persisted in `identity.RefreshTokens`.
* **Sliding Rotation**: On every refresh request (`POST /api/auth/refresh`), the old token is marked as revoked/replaced, and a brand-new token pair is issued.
* **Revocation**: Invoking `POST /api/auth/logout` sets `RevokedAt = DateTime.UtcNow`, permanently invalidating the token.

---

## 2. Password Security (BCrypt)

* **Implementation**: Managed by `BCrypt.Net-Next` (`4.2.0`).
* **Hashing**: `BCrypt.EnhancedHashPassword(password)` automatically generates a cryptographic salt and hashes passwords using adaptive Blowfish algorithms.
* **Verification**: `BCrypt.EnhancedVerify(password, passwordHash)` performs constant-time password verification.

---

## 3. Google Sign-In (OpenID Connect)

Google authentication is integrated via [`GoogleTokenValidator`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/Modules/Identity/Identity.Infrastructure/Authentication/GoogleTokenValidator.cs):
1. Client completes Google One-Tap or Google Sign-In and transmits the resulting `idToken` to `POST /api/auth/google`.
2. `GoogleTokenValidator` calls `GoogleJsonWebSignature.ValidateAsync` using `Google.Apis.Auth` (`1.76.0`).
3. Validation enforces:
   * Client ID matches `Authentication:Google:ClientId`.
   * Issuer matches `accounts.google.com` or `https://accounts.google.com`.
   * Token has not expired (with $\pm 2$ minutes clock tolerance).
   * Email address is marked as verified by Google (`EmailVerified == true`).
4. `IdentityService` locates the existing user or registers a new account, recording the Google external identity in `identity.ExternalLogins`.
5. Returns a standard `AuthResponse` containing JWT access and refresh tokens.

---

## 4. API Authorization & Endpoint Protection

* **Middleware Registration**: Configured in `Identity.Infrastructure/DependencyInjection.cs`:
  ```csharp
  services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer(options =>
      {
          options.TokenValidationParameters = new TokenValidationParameters
          {
              ValidateIssuer = true,
              ValidateAudience = true,
              ValidateLifetime = true,
              ValidateIssuerSigningKey = true,
              ValidIssuer = issuer,
              ValidAudience = audience,
              IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
              ClockSkew = TimeSpan.Zero
          };
      });
  ```
* **Endpoint Attributes**: Protected endpoints specify `[Authorize]`. Unauthorized requests are intercepted by `GlobalExceptionHandler` returning HTTP 401 Unauthorized.

