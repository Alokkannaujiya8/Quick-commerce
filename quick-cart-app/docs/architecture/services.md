# Services Architecture Specification

This document details the service layer, dependency injection mechanisms, singleton lifecycles, and domain responsibilities of all services in the QuickCart frontend.

---

## 1. Service Layer Principles

* **Root Singletons**: Every domain service is registered using `@Injectable({ providedIn: 'root' })`, ensuring a single shared instance throughout the application lifecycle.
* **Modern `inject(...)` Pattern**: Dependencies (`HttpClient`, `Router`, `DOCUMENT`, `NgZone`, sibling services) are resolved using the functional `inject(...)` API rather than constructor injection.
* **Separation of Concerns**: Services own API network communication, data transformations, signal state stores, and local storage caching. Components consume services without interacting directly with `HttpClient` or browser `localStorage`.

---

## 2. Service Inventory

```text
src/app/
├── core/
│   ├── auth/
│   │   ├── auth-service.ts            # Authentication, session, tokens, profile
│   │   └── google-identity.service.ts # Google Identity Services SDK integration
│   └── services/
│       ├── api.service.ts             # Generic HTTP client wrapper
│       └── theme.service.ts           # Reactive theme state & DOM sync
└── modules/
    ├── cart/services/cart.service.ts       # Cart Signal store & computations
    ├── catalog/services/catalog.service.ts # Products & categories catalog API
    ├── orders/services/order.service.ts    # Orders API, checkout, & order history
    └── payments/services/payment.service.ts# Payment intents & verification API
```

---

## 3. Core Services Specification

### 3.1 `ApiService` (`src/app/core/services/api.service.ts`)
* **Role**: Centralized HTTP client wrapper.
* **Dependencies**: `HttpClient` (`inject(HttpClient)`).
* **Methods**:
  * `get<T>(endpoint, params)`: Maps query parameters to `HttpParams`, cleans leading slashes, executes GET, and catches/logs errors.
  * `post<T>(endpoint, body)`: Executes POST request with JSON payload.
  * `put<T>(endpoint, body)`: Executes PUT request.
  * `delete<T>(endpoint)`: Executes DELETE request.
* **Base URL**: Resolved from `environment.apiUrl` (`https://localhost:7189/api`).

### 3.2 `AuthService` (`src/app/core/auth/auth-service.ts`)
* **Role**: Customer identity, session token store, and multi-factor authentication methods.
* **Signals**:
  * `currentUser: Signal<User | null>`
  * `user: Signal<User | null>` (read-only projection)
  * `isAuthenticated: Signal<boolean>`
  * `pendingPhoneNumber: Signal<string>`
* **Methods**:
  * `sendOtp(phoneNumber)`
  * `verifyOtp(phoneNumber, code)`
  * `googleSignIn(idToken)`
  * `register(request)`
  * `login(request)`
  * `refreshToken()`
  * `getMe()`
  * `logout()`
  * `getAccessToken()`, `getRefreshToken()`

### 3.3 `GoogleIdentityService` (`src/app/core/auth/google-identity.service.ts`)
* **Role**: Encapsulates the Google Identity Services (GIS) JavaScript client SDK.
* **Features**:
  * Loads `https://accounts.google.com/gsi/client` asynchronously once.
  * Single initialization via `google.accounts.id.initialize`.
  * Renders hidden or custom Google buttons without calling deprecated One Tap UI methods.
  * Emits ID tokens via RxJS `Observable<string>` with NgZone execution.

### 3.4 `ThemeService` (`src/app/core/services/theme.service.ts`)
* **Role**: Application-wide dark/light theme state.
* **Features**:
  * Detects initial theme from `localStorage` or `prefers-color-scheme`.
  * Runs an Angular `effect()` to synchronize the `data-theme` attribute and `dark-theme` CSS class on `<html>` and `<body>`.
  * Persists selected theme to `localStorage` (`qc_theme`).

---

## 4. Feature Domain Services

### 4.1 `CartService` (`src/app/modules/cart/services/cart.service.ts`)
* **Role**: Reactive shopping cart state store.
* **State**: Angular Signals (`items`, `isDrawerOpen`, `totalItems`, `subtotal`, `deliveryFee`, `totalAmount`).
* **Methods**: `addToCart()`, `updateQuantity()`, `removeItem()`, `clearCart()`, `toggleDrawer()`, `openDrawer()`, `closeDrawer()`.

### 4.2 `CatalogService` (`src/app/modules/catalog/services/catalog.service.ts`)
* **Role**: Catalog retrieval.
* **Methods**: `getCategories()`, `getProducts(categorySlug?, search?)`, `getProductBySlug(slug)`.
* **Resilience**: Contains fallback mock data for offline/demo resilience when the backend is unreachable.

### 4.3 `OrderService` (`src/app/modules/orders/services/order.service.ts`)
* **Role**: Checkout orchestration and order retrieval.
* **Methods**: `createOrder(request)`, `getOrderById(id)`, `getUserOrders()`, `updateOrderPaymentState()`.
* **State**: `currentOrder`, `orderHistory`.

### 4.4 `PaymentService` (`src/app/modules/payments/services/payment.service.ts`)
* **Role**: Payment gateway integration with QuickCart backend.
* **Methods**: `createPaymentIntent()`, `verifyPayment()`, `getPaymentByOrderId()`, `retryPayment()`.
* **State**: `activeIntent`, `latestPayment`.
