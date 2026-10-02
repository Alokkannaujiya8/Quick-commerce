# QuickCart Frontend — Architecture Overview

Welcome to the architectural specification for the **QuickCart** frontend application. This document serves as the high-level entry point into the system design, directory structure, reactivity models, and communication flows of the Angular 20 client application.

---

## 1. Application Overview

**QuickCart** is a modern quick-commerce web application engineered for instant (10-minute) grocery fulfillment. The frontend provides customer catalog browsing, dynamic search, reactive cart management, multi-step checkout, Google OpenID / OTP / Password authentication, and payment gateway settlement.

### High-Level Architectural Highlights
* **Framework**: Angular 20 (`20.3.0`) on Node.js / npm tooling.
* **Component Paradigm**: 100% Standalone Components with no `NgModule` dependencies.
* **Reactivity Engine**: Hybrid reactive architecture utilizing **Angular Signals** for synchronous view and domain state and **RxJS 7.8** for asynchronous network streams.
* **Template Syntax**: Built-in Modern Control Flow (`@if`, `@else`, `@for`, `@switch`).
* **Design & Styling**: Custom responsive SCSS with CSS variable design tokens and dark/light mode switching via `ThemeService`.

---

## 2. System Architecture & Project Blueprint

```text
quick-cart-app/
├── src/
│   ├── app/
│   │   ├── core/                  # Singleton infrastructure: auth, http, guards, models
│   │   │   ├── auth/              # AuthService, GoogleIdentityService, auth.models
│   │   │   ├── guards/            # Functional route guards (authGuard)
│   │   │   ├── interceptors/      # Functional HTTP interceptors (authInterceptor)
│   │   │   ├── models/            # Shared User and system models
│   │   │   └── services/          # ApiService, ThemeService
│   │   ├── layout/                # App shell: MainLayout, Header, Footer
│   │   ├── modules/               # Domain feature modules
│   │   │   ├── cart/              # CartService (Signals), CartDrawerComponent
│   │   │   ├── catalog/           # CatalogService, ProductListComponent, ProductCardComponent
│   │   │   ├── identity/          # Login, Register, OtpVerify, PasswordLogin pages
│   │   │   ├── orders/            # OrderService, CheckoutComponent, OrderSuccessComponent
│   │   │   └── payments/          # PaymentService, Payment models
│   │   ├── shared/                # Presenter UI components, buttons, banners, shells
│   │   ├── app.config.ts          # Application providers (Router, HttpClient, Zone.js)
│   │   ├── app.routes.ts          # Lazy route declarations
│   │   └── app.ts                 # Root shell component (<router-outlet />)
│   ├── environments/              # Environment config (API base URL, Google Client ID)
│   ├── styles.scss                # Global styles and theme tokens
│   └── main.ts                    # Application bootstrap
```

---

## 3. Core Architecture Pillars

### 3.1 Application Bootstrap & Providers
The application bootstraps standalone via `bootstrapApplication(App, appConfig)` in `src/main.ts`.
Providers configured in `src/app/app.config.ts`:
* `provideBrowserGlobalErrorListeners()`: Captures unhandled browser errors.
* `provideZoneChangeDetection({ eventCoalescing: true })`: Coalesces browser microtasks to minimize change detection cycles.
* `provideRouter(routes)`: Establishes lazy-loaded routing based on `app.routes.ts`.
* `provideHttpClient(withInterceptors([authInterceptor]))`: Injects the functional JWT authentication interceptor into all outgoing requests.

### 3.2 Component Architecture
* **Standalone First**: Every component declares `standalone: true` and explicitly declares imports.
* **OnPush Change Detection**: High-performance rendering using `changeDetection: ChangeDetectionStrategy.OnPush` across layout and feature components.
* **Smart vs. Dumb (Presentational) Components**:
  * **Feature Pages** (`CheckoutComponent`, `ProductListComponent`, `LoginComponent`) orchestrate services, handle routing, and manage page-level Signals.
  * **Shared UI** (`UiButtonComponent`, `AlertBannerComponent`, `AuthCardShellComponent`, `BrandLogoComponent`, `ThemeToggleComponent`) act as pure presentation widgets with typed inputs/outputs.

### 3.3 Reactivity & State Management
* **Angular Signals**: Used for synchronous and localized reactive state.
  * `CartService`: `items = signal<CartItem[]>()`, `totalItems = computed(...)`, `totalAmount = computed(...)`.
  * `AuthService`: `currentUser = signal<User | null>()`, `isAuthenticated = computed(...)`.
  * `ThemeService`: `theme = signal<AppTheme>()`, `isDark = computed(...)`.
* **RxJS Pipelines**: Used for asynchronous I/O and server interaction.
  * Multi-step flows use `switchMap`, `map`, and `catchError` (e.g. creating an order, requesting payment intent, verifying signature).

### 3.4 Routing & Navigation Flow
* Configured in `src/app/app.routes.ts` with route-level lazy loading (`loadComponent: () => import(...).then(m => m.Component)`).
* Layout shell hierarchy:
  * `/` (Catalog/Home) -> Wrapped in `MainLayoutComponent` with Header, Footer, and CartDrawer.
  * `/checkout` -> Checkout form & address collection inside `MainLayoutComponent`.
  * `/order-success/:id` -> Order confirmation & payment badge inside `MainLayoutComponent`.
  * `/login`, `/login/verify`, `/login/password`, `/register` -> Dedicated fullscreen auth views.

### 3.5 Authentication & Authorization
* **Dual Auth Modes**:
  1. **Phone OTP**: `sendOtp(phone)` -> `verifyOtp(phone, code)`.
  2. **Email & Password**: `login({ login, password })`.
  3. **Google Identity Services (GIS)**: Popup credential extraction -> `/api/auth/google`.
* **Tokens**: JWT access token stored in `localStorage` (`qc_access_token`) with refresh token (`qc_refresh_token`).
* **Route Protection**: `authGuard` checks `isAuthenticated()` and redirects to `/login`.

### 3.6 HTTP API Communication & Interceptors
* **ApiService**: Generic wrapper around `HttpClient` configuring base URL (`https://localhost:7189/api`), parameter serialization, and error rethrowing.
* **AuthInterceptor**: Injects `Authorization: Bearer <token>` and handles transparent token refresh on `401 Unauthorized`.

### 3.7 Error Handling Strategy
* Client-side UI error banners (`AlertBannerComponent`, `errorMessage` signals).
* RFC 7807 Problem Details inspection (`err?.error?.detail || err?.error?.message`).
* Resilience fallbacks in `CatalogService` and `OrderService` for offline development/demo scenarios.

### 3.8 Build & Tooling
* **Builder**: `@angular/build:application` powered by Vite and esbuild.
* **Budgets**: Initial bundle warning at 500kB, error at 1MB (`angular.json`).
* **Environments**: Automated environment file substitution (`environment.ts` replaced by `environment.development.ts` during local dev).

---

## 4. Documentation Index

For exhaustive engineering and architectural documentation, refer to the following guides:

### Engineering Guides (`docs/engineering/`)
* [RULES.md](docs/engineering/RULES.md) — Coding conventions, TypeScript standards, DI rules.
* [TECH_STACK.md](docs/engineering/TECH_STACK.md) — Verified versions of Angular, TypeScript, bundlers, and libraries.
* [DEPENDENCIES.md](docs/engineering/DEPENDENCIES.md) — Production and development package breakdown.
* [ERROR_HANDLING.md](docs/engineering/ERROR_HANDLING.md) — Problem Details, HTTP status handling, and UI toasts.
* [SECURITY.md](docs/engineering/SECURITY.md) — JWT handling, Google Identity, XSS mitigations.
* [PERFORMANCE.md](docs/engineering/PERFORMANCE.md) — Lazy loading, bundle sizes, OnPush, event coalescing.
* [TESTING.md](docs/engineering/TESTING.md) — Jasmine & Karma setup, unit tests, spec execution.
* [STATE_MANAGEMENT.md](docs/engineering/STATE_MANAGEMENT.md) — Signals, computed values, storage sync, RxJS.
* [OBSERVABILITY.md](docs/engineering/OBSERVABILITY.md) — Logging, telemetry, and error monitoring status.

### Architecture Specifications (`docs/architecture/`)
* [application-structure.md](docs/architecture/application-structure.md) — Detailed folder anatomy and file conventions.
* [components.md](docs/architecture/components.md) — Component inventory and design patterns.
* [services.md](docs/architecture/services.md) — Service boundaries, dependencies, and lifecycle.
* [routing.md](docs/architecture/routing.md) — Route trees, titles, and layout wrapping.
* [authentication-authorization.md](docs/architecture/authentication-authorization.md) — Auth workflows and tokens.
* [http-api.md](docs/architecture/http-api.md) — HTTP client architecture and backend integration contracts.
* [interceptors.md](docs/architecture/interceptors.md) — Interceptor mechanics and token retry flows.
* [guards.md](docs/architecture/guards.md) — Guard implementations and route protection status.
* [state-management.md](docs/architecture/state-management.md) — Deep dive into Signal stores and reactivity.
* [forms-validation.md](docs/architecture/forms-validation.md) — Template-driven forms, phone parsing, and validation.
* [error-handling.md](docs/architecture/error-handling.md) — Architectural error layers and fallbacks.
* [testing.md](docs/architecture/testing.md) — Testing strategy and current test suite analysis.
* [build-deployment.md](docs/architecture/build-deployment.md) — Build configurations, budgets, and distribution.
* [diagrams/](docs/architecture/diagrams/README.md) — Mermaid architecture, auth, checkout, and data flow diagrams.

### Architecture Decision Records (`docs/adr/`)
* [0001-standalone-components-and-control-flow.md](docs/adr/0001-standalone-components-and-control-flow.md)
* [0002-signals-and-rxjs-hybrid-state.md](docs/adr/0002-signals-and-rxjs-hybrid-state.md)
* [0003-jwt-auth-interceptor-with-refresh-rotation.md](docs/adr/0003-jwt-auth-interceptor-with-refresh-rotation.md)
* [0004-google-identity-services-integration.md](docs/adr/0004-google-identity-services-integration.md)

### AI Engineering Context (`docs/ai/`)
* [WORKFLOW.md](docs/ai/WORKFLOW.md) — Autonomous AI operational cycle.
* [CURRENT_STATE.md](docs/ai/CURRENT_STATE.md) — Live state, test audit, verified routes, and known gaps.
* [ENGINEERING_MEMORY.md](docs/ai/ENGINEERING_MEMORY.md) — Durable frontend memory and patterns.
* [DECISIONS.md](docs/ai/DECISIONS.md) — Architectural decisions ledger.
