# Testing Strategy & Test Suites Specification

This document details the test projects, testing frameworks, test conventions, and verified execution status across the QuickCart backend repository.

---

## 1. Test Project Registry & Verified Status

The repository defines 7 test projects under `tests/`. An audit of `.csproj` dependencies, test code files, and runtime execution reveals two categories of projects:

| Test Project | Framework Packages | Test Source Files Present | Verified Test Run Status |
| :--- | :--- | :--- | :--- |
| **`tests/Identity.Tests`** | `xunit` `2.9.3`, `Moq` `4.20.72`, `Microsoft.NET.Test.Sdk` `17.14.1`, `coverlet.collector` `6.0.4` | Yes (5 test classes) | **41 Passed**, 0 Failed (Duration: ~4s) |
| **`tests/Payment.Tests`** | `xunit` `2.9.3`, `Moq` `4.20.72`, `Microsoft.NET.Test.Sdk` `17.14.1`, `coverlet.collector` `6.0.4` | Yes (1 test class) | **14 Passed**, 0 Failed (Duration: ~0.9s) |
| **`tests/Catalog.Tests`** | *None* (Project references only) | No (0 test files) | *Unconfigured skeleton* |
| **`tests/Delivery.Tests`** | *None* (Project references only) | No (0 test files) | *Unconfigured skeleton* |
| **`tests/Inventory.Tests`** | *None* (Project references only) | No (0 test files) | *Unconfigured skeleton* |
| **`tests/Ordering.Tests`** | *None* (Project references only) | No (0 test files) | *Unconfigured skeleton* |
| **`tests/QuickCart.IntegrationTests`** | *None* (Project references only) | No (0 test files) | *Unconfigured skeleton* |

---

## 2. Verified Active Test Suites

### 2.1 Identity.Tests (41 Tests)
* **[`IdentityServiceTests.cs`](file:///d:/Asp.net%20core_Project/Quick-commerce/tests/Identity.Tests/Application/IdentityServiceTests.cs)**:
  * User registration (validation, password hashing, duplicate email/phone checks).
  * User authentication (valid credentials, incorrect password, inactive account handling).
  * OTP generation and verification flows.
  * Refresh token generation, validation, rotation, and revocation.
* **[`GoogleTokenValidatorTests.cs`](file:///d:/Asp.net%20core_Project/Quick-commerce/tests/Identity.Tests/Infrastructure/GoogleTokenValidatorTests.cs)**:
  * Google token signature validation settings.
  * Audience matching and mismatch detection.
  * Issuer verification (`accounts.google.com`).
  * Clock skew tolerance ($\pm 2$ minutes).
  * Unverified email rejection.
  * Development simulated token parser verification.
* **[`JwtTokenServiceTests.cs`](file:///d:/Asp.net%20core_Project/Quick-commerce/tests/Identity.Tests/Infrastructure/JwtTokenServiceTests.cs)**:
  * JWT access token claim generation (`sub`, `jti`, `name`, `email`, `phone`).
  * Token expiration and lifetime calculations.
  * 64-byte CSPRNG refresh token generation.
* **[`ApplicationUserTests.cs`](file:///d:/Asp.net%20core_Project/Quick-commerce/tests/Identity.Tests/Domain/ApplicationUserTests.cs)**:
  * Entity invariants, property initialization, and refresh token attachment.
* **[`AuthControllerGoogleTests.cs`](file:///d:/Asp.net%20core_Project/Quick-commerce/tests/Identity.Tests/Presentation/AuthControllerGoogleTests.cs)**:
  * Google login endpoint HTTP responses (200 OK on valid token, 400 Bad Request on empty token, 401 Unauthorized on invalid signature).

### 2.2 Payment.Tests (14 Tests)
* **[`PaymentServiceTests.cs`](file:///d:/Asp.net%20core_Project/Quick-commerce/tests/Payment.Tests/Services/PaymentServiceTests.cs)**:
  * Server-side order verification (queries `IOrderService`, verifies amount matches authoritative order total).
  * Idempotency handling (replaying payment intent returns existing payment without creating duplicates).
  * IDOR prevention (throws `UnauthorizedAccessException` when order user does not match caller).
  * Payment signature verification (HMAC-SHA256 constant-time comparison).
  * Webhook processing and deduplication (ignoring duplicate provider events).
  * Partial and full refund validation.

---

## 3. Testing Conventions & Architecture

1. **Pattern**: Arrange, Act, Assert (AAA).
2. **Isolation**: Unit tests mock external dependencies using `Moq`. Unit tests must not communicate with a physical PostgreSQL database or external third-party API.
3. **Execution Command**:
   ```powershell
   dotnet test QuickCart.sln
   ```
4. **Targeted Execution**:
   ```powershell
   dotnet test tests/Identity.Tests
   dotnet test tests/Payment.Tests
   ```

