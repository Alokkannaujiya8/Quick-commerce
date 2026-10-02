# HTTP API Communication & Contract Mapping

This document specifies the HTTP client architecture, endpoint routing, and exact contract mappings between the frontend expectations and backend API contracts.

---

## 1. HTTP Client Architecture

* **Provider**: Configured via `provideHttpClient(withInterceptors([authInterceptor]))` in `src/app/app.config.ts`.
* **Gateway**: Centralized through `ApiService` (`src/app/core/services/api.service.ts`), which prepends `environment.apiUrl` (`https://localhost:7189/api`) and handles error re-throwing.

---

## 2. API Contract Mapping & Verification

### 2.1 Identity & Authentication

#### `POST /api/auth/otp/send`
* **Frontend Expectation**: Dispatches OTP to customer phone.
* **HTTP Request**: `POST /api/auth/otp/send` with `{ "phoneNumber": "9876543210" }`.
* **Backend API Contract**: `SendOtpCommand` with `PhoneNumber`.
* **Actual Response**: `200 OK` with `{ "success": true }`.
* **Discrepancy**: None. Aligned.

#### `POST /api/auth/otp/verify`
* **Frontend Expectation**: Verifies code, returns user and tokens.
* **HTTP Request**: `POST /api/auth/otp/verify` with `{ "phoneNumber": "9876543210", "code": "123456" }`.
* **Backend API Contract**: `VerifyOtpCommand` with `PhoneNumber`, `Code`.
* **Actual Response**: `200 OK` with `AuthResponse` (`tokens`, `user`).
* **Discrepancy**: None. Aligned.

#### `POST /api/auth/google`
* **Frontend Expectation**: Submits Google ID token credential.
* **HTTP Request**: `POST /api/auth/google` with `{ "idToken": "..." }`.
* **Backend API Contract**: `GoogleLoginCommand` with `IdToken`.
* **Actual Response**: `200 OK` with `AuthResponse` (`tokens`, `user`).
* **Discrepancy**: None. Aligned.

#### `POST /api/auth/login`
* **Frontend Expectation**: Password login.
* **HTTP Request**: `POST /api/auth/login` with `{ "login": "jane@example.com", "password": "...", "deviceName": "Web Browser" }`.
* **Backend API Contract**: `LoginCommand` with `Login` (or `EmailOrPhone`), `Password`, `DeviceName`.
* **Actual Response**: `200 OK` with `AuthResponse` (`tokens`, `user`).
* **Discrepancy**: None. Aligned.

---

### 2.2 Catalog

#### `GET /api/catalog/categories`
* **Frontend Expectation**: Fetches category tree.
* **HTTP Request**: `GET /api/catalog/categories`.
* **Backend API Contract**: Returns array of `CategoryDto` (`id`, `name`, `slug`, `iconUrl`, `displayOrder`, `subCategories`).
* **Actual Response**: Array of categories.
* **Frontend Fallback**: On network failure, `CatalogService` emits 6 static mock categories (`dairy-bread-eggs`, etc.).

#### `GET /api/catalog/products`
* **Frontend Expectation**: Fetches products filtered by category slug or query text.
* **HTTP Request**: `GET /api/catalog/products?category=...&q=...`.
* **Backend API Contract**: Returns array of `ProductDto`.
* **Actual Response**: Array of products.
* **Frontend Fallback**: On network failure, `CatalogService` filters and returns 8 static in-memory products.

---

### 2.3 Ordering & Checkout

#### `POST /api/orders/checkout`
* **Frontend Expectation**: Submits order with address and items.
* **HTTP Request**:
  ```json
  {
    "deliveryAddressId": "f784e1b8-6a34-4bc5-9c3f-912ab0819fa2",
    "paymentMethod": "UPI",
    "notes": "Deliver to ...",
    "deliveryFee": 40,
    "items": [
      {
        "productId": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
        "productName": "Fresh Farm Whole Milk",
        "sku": "MILK-001",
        "unitPrice": 34,
        "quantity": 2
      }
    ]
  }
  ```
* **Backend API Contract**: `CheckoutOrderCommand` with `DeliveryAddressId`, `PaymentMethod`, `Notes`, `DeliveryFee`, `Items`.
* **Actual Response**: `201 Created` with `BackendOrderDto` (`id`, `orderNumber`, `status`, `totalAmount`, ...).
* **Discrepancy / Known Hardcoding**:
  Frontend `OrderService.createOrder()` currently hardcodes `deliveryAddressId` to `'f784e1b8-6a34-4bc5-9c3f-912ab0819fa2'` because a customer Address Management service is not yet integrated into the checkout view.

---

### 2.4 Payments

#### `POST /api/payments/intents`
* **Frontend Expectation**: Creates payment gateway intent.
* **HTTP Request**: `POST /api/payments/intents` with `{ "orderId": "...", "paymentMethod": "UPI", "idempotencyKey": "..." }`.
* **Backend API Contract**: Returns `PaymentIntentResponse` (`paymentId`, `providerOrderId`, `providerKeyId`, `devVerificationSignature`, ...).
* **Actual Response**: `200 OK` with payment intent.
* **Discrepancy**: None. Aligned.

#### `POST /api/payments/verify`
* **Frontend Expectation**: Cryptographically verifies payment signature.
* **HTTP Request**: `POST /api/payments/verify` with `{ "paymentId": "...", "providerOrderId": "...", "providerPaymentId": "...", "providerSignature": "..." }`.
* **Backend API Contract**: Returns `PaymentDto` with status `Captured`.
* **Actual Response**: `200 OK` with verified payment.
* **Discrepancy**: None. Aligned.

---

### 2.5 Unintegrated Endpoints & Architectural Gaps

#### Cart Synchronization (`/api/cart/*`)
```text
Status: Not Implemented in Frontend

Evidence:
Backend specifies `/api/cart`, `POST /api/cart/items`, `PUT /api/cart/items/{id}`, `DELETE /api/cart/items/{id}` in docs/API_CONTRACTS.md.
Frontend `CartService` maintains cart state exclusively in client-side Signals and `localStorage` (`qc_cart_items`). No network calls are made to backend cart endpoints.
```

#### Real-Time Delivery Tracking WebSocket (`/hubs/delivery-tracking`)
```text
Status: Not Implemented in Frontend

Evidence:
Backend exposes SignalR tracking hub at `/hubs/delivery-tracking`.
Frontend does not install `@microsoft/signalr` and contains no SignalR connection services. Delivery estimation is displayed as static text (10-15 mins).
```
