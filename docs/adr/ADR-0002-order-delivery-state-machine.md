# ADR-0002: 10-Minute Order Fulfillment & Delivery State Machine

## Status
Approved

## Context
In a 10-minute instant delivery workflow, an order progresses from placement to customer handover in minutes. State transitions occur rapidly across automated picking, packing, and rider dispatch. The system requires unambiguous lifecycle states, complete auditability, and real-time customer transparency.

## Decision
1. **Explicit Lifecycle States**:
   $$\text{Placed} \longrightarrow \text{Confirmed} \longrightarrow \text{Packed} \longrightarrow \text{OutForDelivery} \longrightarrow \text{Delivered}$$
   Terminal alternative states: `Cancelled` (from `Placed`/`Confirmed`), `Returned` (from `Delivered`).
2. **Immutable Audit Ledger**: Every state transition appends a record to `ordering.OrderStatusHistories` capturing the previous state, new state, UTC timestamp, and operational notes.
3. **Price & SKU Freezing**: Line items capture immutable snapshots (`ProductNameSnapshot`, `ProductSkuSnapshot`, `UnitPrice`) at checkout time, decoupling historical orders from catalog price revisions.
4. **Real-Time Telemetry Broadcasting**: State transitions and delivery rider GPS telemetry are broadcast via `DeliveryTrackingHub` (SignalR) to subscribed frontend clients.

## Consequences
### Positive
* Strict state validation prevents illegal state transitions (e.g. delivering an unconfirmed order).
* Complete audit trail simplifies customer support disputes and delivery SLAs.
* Instant sub-second UI updates via WebSockets.

### Negative / Trade-offs
* Requires maintaining WebSocket connections for active customer tracking.
* Increased database row insertion volume in `ordering.OrderStatusHistories`.

## Verification & Code References
* Aggregate: [`Order`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/Modules/Ordering/Ordering.Domain/Entities/Order.cs), [`OrderStatusHistory`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/Modules/Ordering/Ordering.Domain/Entities/OrderStatusHistory.cs)
* Enums: [`OrderStatus`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/Modules/Ordering/Ordering.Domain/Enums/OrderStatus.cs)
* Service: [`OrderService`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/Modules/Ordering/Ordering.Infrastructure/Services/OrderService.cs)
* Hub: [`DeliveryTrackingHub`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/Modules/Delivery/Delivery.Presentation/Hubs/DeliveryTrackingHub.cs)

