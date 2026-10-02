# Current State Assessment — QuickCart Frontend

This document records the exact, verified technical state of the QuickCart Angular frontend application as of October 2026.

---

## 1. Environment & Framework Baseline

* **Angular Version**: `20.3.0` (`@angular/core`)
* **Angular CLI & Build**: `20.3.8` (`@angular/build:application`, `@angular/cli`)
* **TypeScript**: `5.9.2`
* **Node.js Runtime**: `v24.11.0`
* **npm Package Manager**: `11.6.1`
* **Application Builder**: esbuild + Vite dev server (`@angular/build:application`)
* **Styles**: SCSS (`inlineStyleLanguage: "scss"`)

---

## 2. Verified Route Inventory

Configured in `src/app/app.routes.ts`:

| Route Path | Component | Layout Container | Title | Lazy Chunk |
| :--- | :--- | :--- | :--- | :--- |
| `/` | `ProductListComponent` | `MainLayoutComponent` | QuickCart - Instant 10-Minute Grocery Delivery | `product-list-component` |
| `/checkout` | `CheckoutComponent` | `MainLayoutComponent` | QuickCart - Checkout & Delivery | `checkout-component` |
| `/order-success/:id` | `OrderSuccessComponent` | `MainLayoutComponent` | QuickCart - Order Confirmed | `order-success-component` |
| `/order-success` | `OrderSuccessComponent` | `MainLayoutComponent` | QuickCart - Order Confirmed | `order-success-component` |
| `/login` | `LoginComponent` | Standalone / Auth Card Shell | QuickCart - Log in or Sign up | `login-component` |
| `/login/verify` | `OtpVerifyComponent` | Standalone / Auth Card Shell | QuickCart - Verify OTP | `otp-verify-component` |
| `/login/password` | `PasswordLoginComponent` | Standalone / Auth Card Shell | QuickCart - Email & Password Sign In | `password-login-component` |
| `/register` | `RegisterComponent` | Standalone / Auth Card Shell | QuickCart - Create Account | `register-component` |
| `**` | Redirect to `/` | N/A | N/A | N/A |

---

## 3. Verified Components & Services Inventory

### Components
* **Layout**: `MainLayoutComponent`, `HeaderComponent`, `FooterComponent`.
* **Feature Pages**: `ProductListComponent`, `CheckoutComponent`, `OrderSuccessComponent`, `LoginComponent`, `OtpVerifyComponent`, `PasswordLoginComponent`, `RegisterComponent`.
* **Presenters & Widgets**: `CartDrawerComponent`, `ProductCardComponent`, `UiButtonComponent`, `AlertBannerComponent`, `AuthCardShellComponent`, `BrandLogoComponent`, `ThemeToggleComponent`.

### Services
* `ApiService`: Centralized HTTP request wrapper with query param serialization.
* `AuthService`: Signal-based customer session store and multi-channel auth client (OTP, Password, Google).
* `GoogleIdentityService`: Google Identity Services (GIS) popup integration with single initialization.
* `ThemeService`: Signal-based reactive dark/light theme switcher with DOM attribute synchronization.
* `CartService`: Signal-based reactive shopping cart store with computed totals and local storage persistence.
* `CatalogService`: Product discovery client with offline/demo static catalog fallbacks.
* `OrderService`: Order placement and order history client with offline mock fallback.
* `PaymentService`: QuickCart payment intent creation and signature verification client.

---

## 4. API Integration & Discrepancies Summary

| Domain | Frontend Implementation | Backend API Endpoint | Discrepancy / Alignment |
| :--- | :--- | :--- | :--- |
| **Auth (OTP)** | `sendOtp`, `verifyOtp` | `/api/auth/otp/send`, `/api/auth/otp/verify` | **Fully Aligned** |
| **Auth (Password)** | `login`, `register` | `/api/auth/login`, `/api/auth/register` | **Fully Aligned** |
| **Auth (Google)** | `googleSignIn` | `/api/auth/google` | **Fully Aligned** |
| **Auth (Refresh)** | `authInterceptor` | `/api/auth/refresh` | **Fully Aligned** |
| **Catalog** | `getCategories`, `getProducts` | `/api/catalog/categories`, `/api/catalog/products` | **Aligned** (with static fallbacks on failure) |
| **Ordering** | `createOrder` | `/api/orders/checkout` | **Discrepancy**: Hardcodes `deliveryAddressId: 'f784e1b8-6a34-4bc5-9c3f-912ab0819fa2'` pending address management UI. Fallback mock generated if server offline. |
| **Payments** | `createPaymentIntent`, `verifyPayment` | `/api/payments/intents`, `/api/payments/verify` | **Fully Aligned** |
| **Cart** | `CartService` (Signals + localStorage) | `/api/cart/*` | **Discrepancy / Unintegrated**: Frontend operates purely client-side; does not call backend cart endpoints. |
| **Tracking** | Static text (10-15 mins) | `/hubs/delivery-tracking` | **Not Implemented**: No SignalR client in frontend. |

---

## 5. Testing & Build Status

* **Build Health**: Compilation succeeded with **zero errors**.
  * Initial transfer bundle: ~98.19 kB (well below 500 kB budget).
* **Test Suite Health**:
  * Total tests: 13
  * Passed: 12 (AuthService, GoogleIdentityService, LoginComponent, PaymentService, App creation)
  * Failed: 1 (`App should render title` in `app.spec.ts` due to initial scaffolding mismatch with `<router-outlet />`).

---

## 6. Known Engineering Items & Gaps

1. **Route Guard Attachment**: `authGuard` is implemented and unit-tested, but not currently assigned to the `/checkout` route in `app.routes.ts`.
2. **Cart API Synchronization**: Shopping cart operates in local storage and does not persist to server-side cart tables.
3. **Delivery Address Management**: Customer addresses are entered in checkout text inputs without saving to customer profile addresses.
4. **Scaffolding Spec Mismatch**: `app.spec.ts` line 21 fails looking for an `<h1>` element not present in `app.html`.
