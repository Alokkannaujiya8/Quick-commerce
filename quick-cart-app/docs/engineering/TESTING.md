# QuickCart Frontend — Testing Framework & Execution Guide

This document details the test framework, test isolation strategies, spec inventory, execution commands, and current test status of the QuickCart frontend.

---

## 1. Testing Framework & Tooling

| Component | Verified Technology & Version | Role |
| :--- | :--- | :--- |
| **Test Runner** | Karma `~6.4.0` | Orchestrates browser execution and reports test results |
| **Assertion Framework** | Jasmine Core `~5.9.0` | BDD test structure (`describe`, `it`, `expect`, `spyOn`) |
| **Builder** | `@angular/build:karma` (`^20.3.8`) | Fast Ahead-of-Time compilation of spec files |
| **Target Browser** | Google Chrome Headless | Automated headless execution via `karma-chrome-launcher` |
| **Coverage Tool** | `karma-coverage ~2.2.0` | Coverage metric collection |

---

## 2. Test Execution Command

To execute the entire frontend test suite in a non-interactive CI/developer environment:
```powershell
npm test -- --watch=false --browsers=ChromeHeadless
```

---

## 3. Verified Spec Files & Coverage Inventory

The frontend repository currently contains 5 unit test spec files under `src/`:

```text
src/
├── app/
│   ├── app.spec.ts                                            # Root application shell spec
│   ├── core/
│   │   └── auth/
│   │       ├── auth-service.spec.ts                           # AuthService unit tests
│   │       └── google-identity.service.spec.ts                # GoogleIdentityService unit tests
│   └── modules/
│       ├── identity/
│       │   └── pages/
│       │       └── login/
│       │           └── login.component.spec.ts                # LoginComponent unit tests
│       └── payments/
│           └── services/
│               └── payment.service.spec.ts                    # PaymentService unit tests
```

---

## 4. Current Test Suite Status & Audit Findings

During the verified test run (`task-118`):
* **Total Executed Tests**: 13
* **Successful Tests**: 12
* **Failed Tests**: 1

### Detailed Breakdown

#### 1. `AuthService` (`auth-service.spec.ts`) — PASS (3/3)
* `should be created` — PASS
* `googleSignIn() should POST idToken to /api/auth/google and update auth signals on success` — PASS
* `googleSignIn() should propagate 401 error and keep user unauthenticated on failure` — PASS

#### 2. `GoogleIdentityService` (`google-identity.service.spec.ts`) — PASS (3/3)
* `should call google.accounts.id.initialize only once when initializeGoogle is called multiple times` — PASS
* `should call google.accounts.id.initialize only once across multiple requestIdToken invocations` — PASS
* `should emit credential when Google callback is invoked` — PASS

#### 3. `LoginComponent` (`login.component.spec.ts`) — PASS (3/3)
* `should create LoginComponent` — PASS
* `should request Google ID token, call authService.googleSignIn, and navigate to home on success` — PASS
* `should display error banner and not navigate when Google authentication fails` — PASS

#### 4. `PaymentService` (`payment.service.spec.ts`) — PASS (2/2)
* `creates a payment intent and updates activeIntent signal` — PASS
* `verifies payment signature and updates latestPayment signal` — PASS

#### 5. `App` (`app.spec.ts`) — 1 PASS, 1 FAIL
* `should create the app` — PASS
* `should render title` — **FAILED**
  * **Failure Reason**: `Expected undefined to contain 'Hello, ecommerce-angular'`.
  * **Root Cause Analysis**: `src/app/app.html` was refactored to contain only `<router-outlet />` as the shell root, but `app.spec.ts` retains the original scaffolding assertion expecting `<h1>Hello, ecommerce-angular</h1>`.

> [!NOTE]
> Per the operating rules, source files were not modified. The failure is recorded here for traceability.

---

## 5. Testing Best Practices & Mocking Patterns

### Mocking HTTP Calls with `provideHttpClientTesting`
Unit tests mock network I/O using Angular's `HttpTestingController`:
```typescript
beforeEach(() => {
  TestBed.configureTestingModule({
    providers: [
      PaymentService,
      provideHttpClient(),
      provideHttpClientTesting()
    ],
  });
  service = TestBed.inject(PaymentService);
  httpMock = TestBed.inject(HttpTestingController);
});

afterEach(() => {
  httpMock.verify(); // Ensures no outstanding requests remain
});
```

### Mocking Services with Jasmine Spies
Components mock dependent services via `jasmine.createSpyObj`:
```typescript
authServiceSpy = jasmine.createSpyObj<AuthService>('AuthService', [
  'sendOtp',
  'googleSignIn',
]);
(authServiceSpy as any).currentUser = signal(null);
```
