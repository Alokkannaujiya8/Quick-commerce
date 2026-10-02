# QuickCart Frontend — AI Agent Guidelines & Operating Rules

This document defines the authoritative operating rules, architectural boundaries, coding conventions, and engineering constraints for AI coding agents and human engineers working in the **QuickCart** Angular frontend (`quick-cart-app`).

---

## 1. Project Overview & Philosophy

* **Domain**: Quick Commerce (Instant Grocery Delivery) frontend providing 10-minute order placement, catalog discovery, real-time cart computation, and payment settlement.
* **Framework**: **Angular 20 (`@angular/core` `^20.3.0`)** with **TypeScript `~5.9.2`**.
* **Paradigm**: Standalone Components (`standalone: true`), Modern Built-in Control Flow (`@if`, `@for`, `@switch`), Hybrid Reactivity (**Angular Signals** for synchronous component/UI/service state, and **RxJS 7.8** for asynchronous HTTP streams and event pipelines).
* **Styling**: SCSS with design token custom properties and dark/light theme switching via `ThemeService`.
* **Host Application**: Bootstrap entry point at `src/main.ts` with providers defined in `src/app/app.config.ts`.

---

## 2. Prime Operating Directives for AI Agents

1. **INSPECT BEFORE MODIFY**:
   * Always audit existing TypeScript code, SCSS stylesheets, component templates, unit tests, and configuration before proposing or applying changes.
   * Verify all types, models, and endpoints against `package.json`, `angular.json`, and backend API contracts.
2. **MODERN ANGULAR SYNTAX ONLY**:
   * Never introduce legacy NgModule imports or legacy structural directives (`*ngIf`, `*ngFor`, `*ngSwitch`) unless maintaining a legacy external library. All application templates must use Angular built-in control flow (`@if`, `@for`, `@switch`).
   * Preserve standalone component architectures.
3. **DO NOT INVENT ARCHITECTURAL CLAIMS**:
   * If a capability is missing (e.g. backend Cart API synchronization, SignalR WebSocket client for real-time delivery, Service Worker offline caching, NgRx store), document it explicitly as:
     ```text
     Status: Not Implemented / Not Confirmed
     Evidence:
     <what was inspected>
     ```
   * Never claim a feature is implemented when it is only mocked or client-side fallback.
4. **REUSE BEFORE CREATE**:
   * Reuse primitives in `src/app/core/services/` (`ApiService`, `AuthService`, `ThemeService`) and shared components in `src/app/shared/components/` (`UiButtonComponent`, `AlertBannerComponent`, `AuthCardShellComponent`, `BrandLogoComponent`, `ThemeToggleComponent`).
5. **DO NOT MODIFY BACKEND FILES**:
   * Backend code (`src/Modules/`, `src/QuickCart.Api/`, `tests/`) must not be modified during frontend tasks.
6. **NEVER PERFORM DESTRUCTIVE GIT ACTIONS**:
   * Prohibited: `git reset`, `git restore`, `git checkout .`, `git clean`, `git stash`, `git rebase`, `git commit`, `git push`.
   * Always preserve uncommitted user changes in the workspace.

---

## 3. Technology Stack Baseline

| Technology | Verified Version | Purpose & Location |
| :--- | :--- | :--- |
| **Angular Framework** | `20.3.0` | Core, Common, Compiler, Forms, Platform-Browser, Router (`package.json`) |
| **Angular CLI & Build**| `20.3.8` | `@angular/build:application` (Vite dev server / esbuild bundler) (`angular.json`) |
| **TypeScript** | `5.9.2` | Configured with `strict: true`, `moduleResolution: "bundler"`, `target: "ES2022"` (`tsconfig.json`) |
| **RxJS** | `~7.8.0` | Asynchronous event streams, HTTP operations, and operators (`package.json`) |
| **Polyfills** | `zone.js ~0.15.0` | Zone.js change detection with `eventCoalescing: true` (`app.config.ts`, `angular.json`) |
| **Testing** | Jasmine `~5.9.0`, Karma `~6.4.0` | Unit test execution with `ChromeHeadless` (`tsconfig.spec.json`, `angular.json`) |
| **Styling** | SCSS (`inlineStyleLanguage: "scss"`) | Global tokens in `src/styles.scss`, scoped component styles |

---

## 4. Folder Structure & Architectural Conventions

The repository follows a clean, modular structure organized by lifecycle and domain responsibility:

```text
quick-cart-app/
├── src/
│   ├── app/
│   │   ├── core/                  # Singleton services, guards, interceptors, core models
│   │   │   ├── auth/              # AuthService, GoogleIdentityService, auth.models
│   │   │   ├── guards/            # Functional route guards (authGuard)
│   │   │   ├── interceptors/      # Functional HTTP interceptors (authInterceptor)
│   │   │   ├── models/            # Core user and system models
│   │   │   └── services/          # Core ApiService, ThemeService, re-exports
│   │   ├── layout/                # Application shell components (Header, Footer, MainLayout)
│   │   ├── modules/               # Domain feature modules (Catalog, Cart, Orders, Payments, Identity)
│   │   │   ├── <feature>/
│   │   │   │   ├── components/    # Feature-specific presenter components
│   │   │   │   ├── models/        # Feature interfaces and DTOs
│   │   │   │   ├── pages/         # Routable view components
│   │   │   │   └── services/      # Feature services and state management
│   │   ├── shared/                # Reusable UI components, pipes, directives
│   │   ├── app.config.ts          # Application-level dependency injection providers
│   │   ├── app.routes.ts          # Root routing configuration (lazy loaded)
│   │   ├── app.ts                 # Root shell component
│   │   └── app.html / app.scss    # Root template (<router-outlet />) and styles
│   ├── environments/              # Environment configurations (development & production)
│   └── main.ts                    # Application bootstrap (bootstrapApplication)
```

---

## 5. Coding & Component Conventions

### Standalone Components
* All components must declare `standalone: true` (or rely on Angular 20 standalone default) and import only required dependencies in `imports: [...]`.
* Use `changeDetection: ChangeDetectionStrategy.OnPush` on all components to optimize change detection performance and align with Angular Signals.

### Built-in Control Flow
* Use native Angular template syntax:
  ```html
  @if (condition()) {
    <p>Rendered</p>
  } @else if (otherCondition()) {
    <p>Alternative</p>
  } @else {
    <p>Fallback</p>
  }

  @for (item of items(); track item.id) {
    <div>{{ item.name }}</div>
  } @empty {
    <p>No items found.</p>
  }
  ```
* Mandatory tracking in `@for`: Always provide a unique tracking key (e.g. `track item.id` or `track $index`).

### Dependency Injection
* Use the modern `inject(...)` function instead of constructor parameter injection for cleaner inheritance and lifecycle handling:
  ```typescript
  export class CheckoutComponent {
    private readonly orderService = inject(OrderService);
    private readonly cart = inject(CartService);
  }
  ```

---

## 6. Reactivity & State Management Conventions

* **Angular Signals for UI & Synchronous State**:
  * Use `signal<T>()` for mutable state (e.g. `isSubmitting`, `errorMessage`, `activeCategory`).
  * Use `computed(() => ...)` for derived state (e.g. `totalItems`, `subtotal`, `deliveryFee`).
  * Use `effect(() => ...)` only for side-effects like syncing state to local storage or manipulating document attributes (e.g. `ThemeService.applyThemeToDom`).
  * Expose read-only signals externally using `.asReadonly()` when protecting internal writable state.
* **RxJS for Asynchronous I/O**:
  * Use `Observable<T>` for HTTP client calls (`ApiService`, `HttpClient`).
  * Handle errors with RxJS operators (`catchError`, `throwError`).
  * Use operators like `switchMap`, `tap`, `map` to orchestrate multi-step asynchronous processes.
  * Subscribe using object syntax: `.subscribe({ next: (res) => {}, error: (err) => {} })`.

---

## 7. HTTP Communication, Interceptors & Security Rules

* **Base API Service**: All domain feature services must call `ApiService` or `HttpClient` with standard typing.
* **Authentication Interceptor (`authInterceptor`)**:
  * Injects `Authorization: Bearer <token>` into outbound HTTP requests when an access token exists in `localStorage` (`qc_access_token`).
  * Excludes auth endpoints (`/auth/login`, `/auth/register`, `/auth/google`, `/auth/otp/`, `/auth/refresh`) from automatic retry loops.
  * On `401 Unauthorized`, automatically invokes `authService.refreshToken()` and retries the original request with the fresh token.
* **Google Identity Services**:
  * Managed via `GoogleIdentityService`.
  * Initialized once per application lifecycle via `google.accounts.id.initialize`.
  * Never call deprecated One Tap UI methods (`isNotDisplayed`, `isSkippedMoment`).
* **Route Guards (`authGuard`)**:
  * Functional `CanActivateFn` that evaluates `authService.isAuthenticated()`.
  * Redirects unauthenticated users to `/login` via `router.createUrlTree(['/login'])`.

---

## 8. Error Handling & Validation Rules

* **Server Error Responses**: The backend returns RFC 7807 Problem Details (`application/problem+json`). Error handling code must check:
  ```typescript
  const errorMsg = err?.error?.detail || err?.error?.message || 'Default fallback message';
  ```
* **Offline / Resilience Fallbacks**: Feature services (`CatalogService`, `OrderService`) implement fallback mock responses on network failure to ensure interactive demo resilience. Document these clearly as fallbacks.
* **Form Validation**: Clean validation messages must be presented to users using `AlertBannerComponent` or inline error blocks.

---

## 9. Testing Rules & Conventions

* **Test Runner**: Karma with Jasmine executing on ChromeHeadless (`npm test -- --watch=false --browsers=ChromeHeadless`).
* **Isolation**: Unit tests must use `provideHttpClientTesting()` and `HttpTestingController` to mock backend endpoints.
* **Signals & DOM Testing**: When testing components using Signals, trigger `fixture.detectChanges()` to evaluate DOM bindings.

---

## 10. AI Workflow & Git Safety Rules

* **Audit First**: Read files thoroughly before writing code.
* **Preserve Working Tree**: Do not discard user changes.
* **No Git Commits or Pushes**: Leave committing and pushing to the repository owner.
* **Follow Verification Protocol**: Verify application compilation via `npm run build` and report test results accurately.
