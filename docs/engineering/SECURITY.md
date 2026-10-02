# Security & Authentication Specification

This document details the security architecture, authentication mechanisms, token lifecycles, cryptographic operations, authorization policies, and webhook validation implemented across QuickCart.

---

## 1. Authentication Architecture

QuickCart implements a **dual-token authentication model** combining short-lived signed JSON Web Tokens (JWT) for stateless API authorization and cryptographically hashed, rotating refresh tokens for session persistence.

```text
┌────────┐               ┌───────────────┐               ┌───────────────┐
│ Client │               │ QuickCart.Api │               │ PostgreSQL    │
└───┬────┘               └───────┬───────┘               └───────┬───────┘
    │  1. POST /login or /google │                               │
    ├───────────────────────────>│  2. Verify credentials/token   │
    │                            │     (BCrypt or Google Auth)   │
    │                            │                               │
    │                            │  3. Generate JWT & Refresh    │
    │                            │     Token (64-byte CSPRNG)    │
    │                            │                               │
    │                            │  4. Store SHA-256 TokenHash   │
    │                            ├──────────────────────────────>│
    │                            │                               │
    │  5. Return TokenResponse   │                               │
    │<───────────────────────────┤                               │
    │     (AccessToken, Expires, │                               │
    │      RefreshToken, Expires)│                               │
```

### 1.1 Access Tokens (JWT)
* **Generator**: [`JwtTokenService`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/Modules/Identity/Identity.Infrastructure/Authentication/JwtTokenService.cs)
* **Algorithm**: HMAC-SHA256 (`SecurityAlgorithms.HmacSha256`)
* **Default Lifetime**: 15 minutes (`Jwt:AccessTokenMinutes`)
* **Standard Claims**:
  * `sub`: User ID (`Guid`)
  * `jti`: Unique Token ID (`Guid`)
  * `ClaimTypes.NameIdentifier`: User ID (`Guid`)
  * `ClaimTypes.Name`: User Full Name
  * `ClaimTypes.MobilePhone`: Verified phone number
  * `ClaimTypes.Email`: Verified email address

### 1.2 Refresh Tokens & Rotation
* **Token Generation**: Generated using a cryptographically secure pseudo-random number generator:
  ```csharp
  Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
  ```
* **Default Lifetime**: 30 days (`Jwt:RefreshTokenDays`)
* **Database Storage**: Raw tokens are **never stored** in the database. Only their SHA-256 digest (`TokenHash`) is stored in `identity.RefreshTokens`.
* **Automatic Rotation**: Calling `POST /api/auth/refresh` validates the existing token hash, marks the old token as replaced/revoked, and persists a newly generated refresh token.
* **Revocation on Logout**: Calling `POST /api/auth/logout` sets `RevokedAt = DateTime.UtcNow`.

### 1.3 Password Hashing
* **Library**: `BCrypt.Net-Next` (`4.2.0`)
* **Implementation**: Uses `BCrypt.EnhancedHashPassword(password)` for hashing and `BCrypt.EnhancedVerify(password, hash)` for verification.
* **Salt Management**: Salts are automatically generated and embedded into the BCrypt hash string.

### 1.4 External Google Sign-In
* **Validator**: [`GoogleTokenValidator`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/Modules/Identity/Identity.Infrastructure/Authentication/GoogleTokenValidator.cs)
* **SDK**: `Google.Apis.Auth` (`GoogleJsonWebSignature`)
* **Security Checks**:
  1. Validates cryptographic signature against Google's public key JWKS endpoints.
  2. Enforces configured audience matching (`Authentication:Google:ClientId`).
  3. Verifies token issuer is `accounts.google.com` or `https://accounts.google.com`.
  4. Enforces clock skew tolerance (configured to $\pm 2$ minutes).
  5. Enforces that Google-reported `EmailVerified` is `true`.
  6. Rejects expired tokens.
* **Development Bypass**: When `Authentication:Google:AllowDevSimulatedTokens` is `true` in `appsettings.Development.json`, tokens prefixed with `dev_google_id_token:` are parsed for local developer simulation. This is disabled in production.

---

## 2. Authorization & Middleware

* **JWT Bearer Validation**: Configured in `Identity.Infrastructure/DependencyInjection.cs`:
  ```csharp
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
  ```
* **Protected Endpoints**: Controllers and action methods enforce access via `[Authorize]`. Unauthenticated requests yield HTTP 401 Unauthorized via `GlobalExceptionHandler`.

---

## 3. Webhook Security & Idempotency

Implemented in the `Payment` module:

### 3.1 Signature Verification
* Inbound payment webhooks (`POST /api/payments/webhook`) must supply the `X-Payment-Signature` header.
* `PaymentGatewayService.VerifyWebhookSignature` recomputes the HMAC-SHA256 signature over the raw request payload using `PaymentGateway:WebhookSecret`.
* Verification uses constant-time string comparison (`CryptographicOperations.FixedTimeEquals`) to eliminate timing attack vectors.

### 3.2 Webhook Deduplication
* Every incoming webhook's payload is hashed with SHA-256 (`PayloadHash`).
* The provider event ID (`ProviderEventId`) is queried against `payment.PaymentWebhookEvents`.
* Duplicate events are safely ignored without re-triggering order status updates.

---

## 4. Insecure Direct Object Reference (IDOR) Protection

* **Order Ownership**: When initiating or verifying payments, `PaymentService` compares the caller's JWT user ID against the target order's `UserId`:
  ```csharp
  if (userId != Guid.Empty && order.UserId != userId)
      throw new UnauthorizedAccessException("Order does not belong to the current user.");
  ```
* **Cart Ownership**: `CartService` resolves the cart exclusively by the authenticated user's ID.
* **Order History**: `OrderService.GetUserOrdersAsync` restricts queries strictly to the caller's `UserId`.

---

## 5. Secret Management & Configuration

* **Runtime Secrets**:
  * `Jwt:SecretKey`: Must be at least 256 bits (32 bytes).
  * `PaymentGateway:WebhookSecret`: Shared secret with the payment gateway.
  * `Authentication:Google:ClientId`: Google Cloud OAuth client ID.
  * `ConnectionStrings:QuickCartDb`: PostgreSQL credentials.
* **Rules**: Production environments must supply secrets via environment variables or secret vaults. Never commit production credentials into source control.

