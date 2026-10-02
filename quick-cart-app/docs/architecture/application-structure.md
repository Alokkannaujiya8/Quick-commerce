# Application Structure Specification

This document details the folder taxonomy, architectural boundaries, and naming conventions used across the QuickCart frontend application.

---

## 1. Directory Tree Overview

```text
quick-cart-app/
├── src/
│   ├── app/
│   │   ├── core/                  # Application-wide singletons, security, and networking
│   │   │   ├── auth/              # Authentication domain services and models
│   │   │   ├── guards/            # Functional routing guards
│   │   │   ├── interceptors/      # Functional HTTP interceptors
│   │   │   ├── models/            # Core user and cross-cutting models
│   │   │   └── services/          # Core services (ApiService, ThemeService)
│   │   ├── layout/                # Persistent application frame components
│   │   │   ├── footer/            # FooterComponent
│   │   │   ├── header/            # HeaderComponent
│   │   │   └── main-layout/       # MainLayoutComponent (App Shell with router-outlet)
│   │   ├── modules/               # Domain feature folders
│   │   │   ├── cart/              # Cart domain (CartService, CartDrawerComponent)
│   │   │   ├── catalog/           # Catalog domain (CatalogService, ProductListComponent)
│   │   │   ├── identity/          # Identity pages (Login, Register, OtpVerify, PasswordLogin)
│   │   │   ├── inventory/         # Inventory domain (placeholders)
│   │   │   ├── orders/            # Orders domain (OrderService, Checkout, OrderSuccess)
│   │   │   └── payments/          # Payments domain (PaymentService, payment models)
│   │   ├── shared/                # Presenter components, alert banners, buttons, logo
│   │   │   ├── components/
│   │   │   ├── directives/
│   │   │   ├── models/
│   │   │   └── pipes/
│   │   ├── app.config.ts          # Core dependency injection providers
│   │   ├── app.routes.ts          # Root routing map
│   │   ├── app.ts                 # Root shell component
│   │   ├── app.html               # Shell template (<router-outlet />)
│   │   ├── app.scss               # Shell stylesheet
│   │   └── app.spec.ts            # Root shell unit test
│   ├── assets/                    # Static images, icons, and illustrations
│   ├── environments/              # Environment configurations
│   ├── favicon.ico
│   ├── index.html                 # HTML entry host
│   ├── main.ts                    # Bootstrap script
│   └── styles.scss                # Global CSS variables and styling tokens
├── angular.json                   # Angular CLI workspace configuration
├── package.json                   # Dependencies, scripts, and project metadata
├── tsconfig.json                  # Root TypeScript configuration
├── tsconfig.app.json              # Application compilation options
└── tsconfig.spec.json             # Test compilation options
```

---

## 2. Directory Responsibilities

### `src/app/core/`
Contains code that is instantiated once during the application lifecycle.
* **`auth/`**: `AuthService`, `GoogleIdentityService`, and `auth.models.ts`. Responsible for token storage, credential negotiation, and session tracking.
* **`guards/`**: `authGuard` functional guard.
* **`interceptors/`**: `authInterceptor` functional HTTP interceptor.
* **`services/`**: `ApiService` (standardized HTTP wrapper) and `ThemeService` (reactive dark/light mode engine).

### `src/app/layout/`
Contains structural shell components:
* `MainLayoutComponent`: Composes `HeaderComponent`, `<router-outlet />`, `CartDrawerComponent`, and `FooterComponent`.
* `HeaderComponent`: Renders brand logo, 10-minute delivery badge, search bar, theme toggle, user greeting/sign-in button, and cart counter.
* `FooterComponent`: Renders footer navigation, delivery guarantees, and copyright.

### `src/app/modules/`
Autonomous feature slices representing business capabilities:
* **`catalog/`**: Product discovery, category navigation chips, and product cards.
* **`cart/`**: Slide-out cart drawer, quantity adjustment, subtotal computation.
* **`orders/`**: Multi-step checkout form, recipient address entry, and order success receipt.
* **`payments/`**: Payment intent dispatch, signature verification, and payment status tracking.
* **`identity/`**: Multi-factor customer login (Phone OTP, Email/Password, Google Sign-In) and registration.
* **`inventory/`**: Reserved for store stock checks (`Status: Not Implemented / Not Confirmed`).

### `src/app/shared/`
Pure presentational components and reusable utilities:
* `UiButtonComponent`: Configurable buttons with primary/secondary/outline variants and loading spinners.
* `AlertBannerComponent`: Dismissible success/danger status alerts.
* `AuthCardShellComponent`: Visual layout container for authentication views.
* `BrandLogoComponent`: QuickCart SVG logo with variant and size parameters.
* `ThemeToggleComponent`: Accessible toggle switch between light and dark mode.

---

## 3. File Naming Conventions

* Components: `<feature>.component.ts`, `<feature>.component.html`, `<feature>.component.scss`.
* Services: `<feature>.service.ts`.
* Models: `<feature>.model.ts` or `<feature>.models.ts`.
* Tests: `<feature>.spec.ts` or `<feature>.component.spec.ts`.
* Routes: `app.routes.ts`.
* Configuration: `app.config.ts`.
