# QuickCart API Contracts Specification

This document details the HTTP REST API endpoints, query parameters, request/response JSON contracts, and WebSocket/SignalR contracts for **QuickCart**.

---

## 1. General API Conventions

* **Base URL**: `https://localhost:7189/api` (Local Dev)
* **Content Type**: `application/json`
* **Date-Time Format**: ISO 8601 UTC (`yyyy-MM-ddTHH:mm:ssZ`)
* **Standard Response Envelopes**:
  * Success: HTTP `200 OK`, `201 Created`, or `204 No Content`.
  * Errors: RFC 7807 **Problem Details** format (`application/problem+json`).

### Standard Error Response (RFC 7807)
```json
{
  "type": "https://quickcart.com/errors/validation",
  "title": "Validation Failed",
  "status": 400,
  "detail": "One or more fields failed validation.",
  "errors": {
    "Quantity": ["Quantity must be greater than 0."]
  },
  "instance": "/api/cart/items"
}
```

---

## 2. Authentication & Identity Endpoints

### `POST /api/auth/register`
Creates a new customer account.

**Request**:
```json
{
  "fullName": "Jane Doe",
  "email": "jane@example.com",
  "phoneNumber": "+919876543210",
  "password": "StrongPassword123!"
}
```

**Response (201 Created)**:
```json
{
  "id": "c1f7b880-9289-4b6e-826f-4dcbf296f011",
  "fullName": "Jane Doe",
  "email": "jane@example.com",
  "phoneNumber": "+919876543210",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

### `POST /api/auth/login`
Authenticates a user via email/phone and password.

**Request**:
```json
{
  "emailOrPhone": "jane@example.com",
  "password": "StrongPassword123!"
}
```

**Response (200 OK)**:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "c1f7b880-9289-4b6e-826f-4dcbf296f011",
    "fullName": "Jane Doe",
    "email": "jane@example.com"
  }
}
```

---

## 3. Catalog Endpoints

### `GET /api/catalog/categories`
Retrieves all product categories with their subcategories and display icons.

**Response (200 OK)**:
```json
[
  {
    "id": "b3e0c01a-6cb2-4a0d-9ff8-3aa45c71b619",
    "name": "Dairy, Bread & Eggs",
    "slug": "dairy-bread-eggs",
    "iconUrl": "🥛",
    "displayOrder": 1,
    "subCategories": [
      {
        "id": "e4f8d22b-7dc3-5b1e-aff9-4bb56d82c720",
        "name": "Milk & Cream",
        "slug": "milk-cream"
      }
    ]
  }
]
```

### `GET /api/catalog/products`
Retrieves products filtered by category slug or search query.

**Query Parameters**:
* `category` (optional, string): Filter by category slug (e.g. `fruits-vegetables`).
* `q` (optional, string): Search query across product name or description.
* `page` (optional, int, default: 1): Page number.
* `pageSize` (optional, int, default: 20): Items per page.

**Response (200 OK)**:
```json
{
  "items": [
    {
      "id": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
      "name": "Fresh Farm Whole Milk",
      "slug": "fresh-farm-whole-milk",
      "description": "Pasteurized whole milk rich in calcium and vitamins.",
      "sku": "MILK-001",
      "unitOfMeasure": "500 ml",
      "price": 34.00,
      "originalPrice": 38.00,
      "imageUrl": "https://images.unsplash.com/photo-1550583724-b2692b85b150",
      "categoryName": "dairy-bread-eggs",
      "isActive": true
    }
  ],
  "totalCount": 42,
  "page": 1,
  "pageSize": 20
}
```

---

## 4. Cart Endpoints

### `GET /api/cart`
Retrieves the active cart for the authenticated user.

**Headers**: `Authorization: Bearer <token>`

**Response (200 OK)**:
```json
{
  "id": "7b0a1d94-52d3-4682-8bc1-12c8b7468112",
  "items": [
    {
      "id": "d134b22c-80ea-4091-8c44-b0451cfbf440",
      "productId": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
      "name": "Fresh Farm Whole Milk",
      "price": 34.00,
      "quantity": 2,
      "lineTotal": 68.00
    }
  ],
  "subtotal": 68.00,
  "deliveryFee": 40.00,
  "totalAmount": 108.00
}
```

### `POST /api/cart/items`
Adds an item to the shopping cart.

**Request**:
```json
{
  "productId": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
  "quantity": 1
}
```

---

## 5. Ordering & Checkout Endpoints

### `POST /api/orders/checkout`
Places an order from the active cart.

**Headers**: `Authorization: Bearer <token>`

**Request**:
```json
{
  "deliveryAddressId": "f784e1b8-6a34-4bc5-9c3f-912ab0819fa2",
  "paymentMethod": "CashOnDelivery"
}
```

**Response (201 Created)**:
```json
{
  "id": "5fa23d11-6548-43bb-81d3-f0a51e60472e",
  "orderNumber": "QC-202609-1094",
  "status": "Placed",
  "subtotal": 68.00,
  "deliveryFee": 40.00,
  "discountAmount": 0.00,
  "totalAmount": 108.00,
  "estimatedDeliveryMinutes": 10,
  "placedAt": "2026-09-12T23:30:00Z"
}
```

### `GET /api/orders/{id}`
Retrieves details and real-time status of an order.

**Response (200 OK)**:
```json
{
  "id": "5fa23d11-6548-43bb-81d3-f0a51e60472e",
  "orderNumber": "QC-202609-1094",
  "status": "OutForDelivery",
  "totalAmount": 108.00,
  "placedAt": "2026-09-12T23:30:00Z",
  "statusHistory": [
    { "status": "Placed", "changedAt": "2026-09-12T23:30:00Z" },
    { "status": "Confirmed", "changedAt": "2026-09-12T23:30:45Z" },
    { "status": "Packed", "changedAt": "2026-09-12T23:32:10Z" },
    { "status": "OutForDelivery", "changedAt": "2026-09-12T23:33:00Z" }
  ]
}
```

---

## 6. Real-Time SignalR Tracking Contracts

* **Hub Endpoint**: `/hubs/delivery-tracking`
* **Protocol**: WebSockets / Long-Polling fallback

### Client to Server Invocations
* `JoinOrderTrackingGroup(string orderId)`: Subscribes the client to live updates for an active order.
* `LeaveOrderTrackingGroup(string orderId)`: Unsubscribes from live updates.

### Server to Client Broadcast Events
* `ReceiveOrderStatusUpdate(string orderId, string newStatus)`: Dispatched when an order state transitions.
* `ReceiveRiderLocationUpdate(string orderId, double latitude, double longitude, int etaMinutes)`: Streamed periodically as the delivery rider moves towards the delivery address.


---

## 7. Payment & Webhook Endpoints

### `POST /api/payments/intents`
Creates an idempotent payment intent using the server-verified order total from `Ordering`.

**Request**:
```json
{
  "orderId": "5fa23d11-6548-43bb-81d3-f0a51e60472e",
  "paymentMethod": "UPI",
  "idempotencyKey": "idem-5fa23d11-UPI"
}
```

**Response (201 Created / 200 OK on idempotent replay)**:
```json
{
  "paymentId": "8c11e320-12a4-4f99-9a00-71b83c992104",
  "orderId": "5fa23d11-6548-43bb-81d3-f0a51e60472e",
  "amount": 108.00,
  "currency": "INR",
  "paymentMethod": "UPI",
  "status": "Initiated",
  "providerName": "QuickCartPay",
  "providerOrderId": "order_9f81c2b3d4e5",
  "providerKeyId": "qcp_test_local_dev",
  "isIdempotentReplay": false,
  "createdAt": "2026-09-27T08:00:00Z"
}
```

### `POST /api/payments/verify`
Verifies checkout payment signature using constant-time `HMAC-SHA256`, captures the payment, and transitions the order to `Confirmed`.

### `POST /api/payments/webhook`
Server-to-server payment provider webhook verified via `X-Payment-Signature` (`HMAC-SHA256`) and deduplicated via `payment."PaymentWebhookEvents"` (`ProviderEventId`).

### `GET /api/payments/order/{orderId}`
Retrieves payment details and audit trail for a given order.

### `POST /api/payments/{id}/retry`
Re-initiates a failed payment with a new provider order reference and increments `RetryCount`.

### `POST /api/payments/{id}/refund`
Processes a partial or full refund with immutable audit logging.
