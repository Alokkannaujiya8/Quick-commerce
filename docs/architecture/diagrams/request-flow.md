# Request Flow Sequence Diagrams

This document contains sequence diagrams illustrating the end-to-end request and event execution flows for order checkout, payment verification, and real-time delivery telemetry.

---

## 1. Order Checkout & Payment Verification Flow

```mermaid
sequenceDiagram
    autonumber
    actor Customer as Customer (SPA)
    participant OrdersCtrl as OrdersController
    participant OrderSvc as OrderService
    participant PayCtrl as PaymentsController
    participant PaySvc as PaymentService
    participant Gateway as PaymentGatewayService
    participant DB as PostgreSQL (Multi-Schema)

    Note over Customer,DB: 1. Checkout Phase
    Customer->>OrdersCtrl: POST /api/orders/checkout (Address, Method, Items)
    OrdersCtrl->>OrderSvc: CreateOrderAsync(userId, request)
    OrderSvc->>DB: INSERT INTO ordering.Orders (Status='Placed')
    OrderSvc->>DB: INSERT INTO ordering.OrderItems
    OrderSvc->>DB: INSERT INTO ordering.OrderStatusHistories
    OrderSvc-->>OrdersCtrl: OrderDto
    OrdersCtrl-->>Customer: 201 Created (OrderDto)

    Note over Customer,DB: 2. Payment Intent Phase
    Customer->>PayCtrl: POST /api/payments/intents (OrderId, IdempotencyKey)
    PayCtrl->>PaySvc: CreatePaymentIntentAsync(request)
    PaySvc->>OrderSvc: GetOrderByIdAsync(orderId)
    OrderSvc-->>PaySvc: Authoritative OrderDto
    PaySvc->>PaySvc: Verify ownership (userId == order.UserId)
    PaySvc->>Gateway: CreateGatewayOrderAsync(orderId, amount)
    Gateway-->>PaySvc: GatewayOrderCreationResult
    PaySvc->>DB: INSERT INTO payment.Payments (Status='Initiated')
    PaySvc-->>PayCtrl: PaymentDto
    PayCtrl-->>Customer: 201 Created (PaymentDto with ProviderOrderId)

    Note over Customer,DB: 3. Payment Verification Phase
    Customer->>PayCtrl: POST /api/payments/verify (Signature, ProviderOrderId)
    PayCtrl->>PaySvc: VerifyPaymentAsync(request)
    PaySvc->>Gateway: VerifyPaymentSignature(providerOrderId, paymentId, signature)
    Gateway-->>PaySvc: Signature Valid (HMAC-SHA256 constant-time check)
    PaySvc->>DB: UPDATE payment.Payments (Status='Captured')
    PaySvc->>DB: INSERT INTO payment.PaymentAuditLogs (Event='PaymentSignatureVerified')
    PaySvc->>OrderSvc: UpdateOrderStatusAsync(orderId, 'Confirmed')
    OrderSvc->>DB: UPDATE ordering.Orders (Status='Confirmed')
    OrderSvc->>DB: INSERT INTO ordering.OrderStatusHistories
    PaySvc-->>PayCtrl: PaymentDto (Captured)
    PayCtrl-->>Customer: 200 OK (Payment Verified)
```

---

## 2. Real-Time Delivery Tracking Flow

```mermaid
sequenceDiagram
    autonumber
    actor Customer as Customer (SPA)
    participant Hub as DeliveryTrackingHub (/hubs/delivery-tracking)
    participant DeliverySvc as DeliveryService
    actor Rider as Delivery Partner App
    participant DB as PostgreSQL (delivery schema)

    Customer->>Hub: WebSocket Connect & JoinOrderTrackingGroup(orderId)
    Hub-->>Customer: Subscribed to group: orderId

    Note over Rider,Customer: Rider streams GPS location
    Rider->>DeliverySvc: RecordLocation(orderId, latitude, longitude)
    DeliverySvc->>DB: INSERT INTO delivery.DeliveryTrackings
    DeliverySvc->>Hub: Broadcast ReceiveRiderLocationUpdate(orderId, lat, lng, etaMinutes)
    Hub-->>Customer: ReceiveRiderLocationUpdate(orderId, lat, lng, etaMinutes)

    Note over Rider,Customer: Order status transition
    Rider->>DeliverySvc: MarkDelivered(orderId)
    DeliverySvc->>Hub: Broadcast ReceiveOrderStatusUpdate(orderId, 'Delivered')
    Hub-->>Customer: ReceiveOrderStatusUpdate(orderId, 'Delivered')
```

