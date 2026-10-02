# Testing Architecture Specification

This document details the unit and component testing architecture, mocking strategies, spec inventory, and execution status of the QuickCart frontend.

---

## 1. Testing Framework & Harness

* **Test Framework**: Jasmine Core `~5.9.0` with `@types/jasmine ~5.1.0`.
* **Runner & Launcher**: Karma `~6.4.0` running against headless Chrome (`karma-chrome-launcher`).
* **Test Compilation**: `@angular/build:karma` builder executing Ahead-of-Time compilation via `tsconfig.spec.json`.
* **Angular Testing Utilities**:
  * `TestBed`: Configures dynamic dependency injection containers per spec suite.
  * `provideHttpClientTesting()` / `HttpTestingController`: Simulates HTTP responses, mocks network latency, and verifies request payloads without live network calls.
  * `ComponentFixture`: Inspects component instances, triggers change detection (`fixture.detectChanges()`), and queries DOM nodes.

---

## 2. Test File Inventory & Classification

```text
quick-cart-app/src/
├── app/
│   ├── app.spec.ts                                            # Shell Component Spec
│   ├── core/
│   │   └── auth/
│   │       ├── auth-service.spec.ts                           # Domain Service Spec (HTTP Testing)
│   │       └── google-identity.service.spec.ts                # SDK Integration Spec (Window Mocking)
│   └── modules/
│       ├── identity/
│       │   └── pages/
│       │       └── login/
│       │           └── login.component.spec.ts                # Container Component Spec (Spy Mocks)
│       └── payments/
│           └── services/
│               └── payment.service.spec.ts                    # Service Spec (HTTP Testing)
```

---

## 3. Unit Test Status Breakdown

Verified via execution command:
```powershell
npm test -- --watch=false --browsers=ChromeHeadless
```

### Result Summary
* **Total Executed Tests**: 13
* **Passed**: 12
* **Failed**: 1

### Detailed Test Results

| Spec Suite | Test Name | Result | Notes |
| :--- | :--- | :--- | :--- |
| `AuthService` | `should be created` | **PASS** | Validates singleton instantiation. |
| `AuthService` | `googleSignIn() should POST idToken to /api/auth/google and update auth signals on success` | **PASS** | Validates HTTP POST body and Signal updates. |
| `AuthService` | `googleSignIn() should propagate 401 error and keep user unauthenticated on failure` | **PASS** | Validates error propagation and state protection. |
| `GoogleIdentityService` | `should call google.accounts.id.initialize only once when initializeGoogle is called multiple times` | **PASS** | Validates deduplicated initialization. |
| `GoogleIdentityService` | `should call google.accounts.id.initialize only once across multiple requestIdToken invocations` | **PASS** | Validates subscriber deduplication. |
| `GoogleIdentityService` | `should emit credential when Google callback is invoked` | **PASS** | Validates token emission to observer. |
| `LoginComponent` | `should create LoginComponent` | **PASS** | Validates component creation. |
| `LoginComponent` | `should request Google ID token, call authService.googleSignIn, and navigate to home on success` | **PASS** | Validates end-to-end component auth flow. |
| `LoginComponent` | `should display error banner and not navigate when Google authentication fails` | **PASS** | Validates error banner activation. |
| `PaymentService` | `creates a payment intent and updates activeIntent signal` | **PASS** | Validates intent POST and signal update. |
| `PaymentService` | `verifies payment signature and updates latestPayment signal` | **PASS** | Validates signature verify POST. |
| `App` | `should create the app` | **PASS** | Validates root shell instantiation. |
| `App` | `should render title` | **FAIL** | Scaffolding test looking for `<h1>Hello, ecommerce-angular</h1>`, while `app.html` contains `<router-outlet />`. |

---

## 4. Uncovered Test Areas (Opportunities for Future Specs)

```text
Status: Not Yet Implemented in Spec Suite

Evidence:
Audited files under src/app/. The following areas currently lack dedicated *.spec.ts files:
- `auth.guard.ts` (Guard logic)
- `auth.interceptor.ts` (Interceptor retry logic)
- `cart.service.ts` (Cart state calculations)
- `catalog.service.ts` (Catalog API & fallbacks)
- `order.service.ts` (Order creation & history)
- `checkout.component.ts` (Checkout view)
- `order-success.component.ts` (Order confirmation view)
- `theme.service.ts` (Theme switching logic)
```
