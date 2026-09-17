# Architecture Decision Record: 10-Minute Order & Delivery State Machine

## Status
Approved

## Context
In a 10-minute instant delivery workflow, orders transition rapidly from placement to fulfillment. Auditing transitions and broadcasting state changes to the customer in real-time is crucial.

## Decision
1. Orders follow an explicit state machine:
   `Placed` -> `Confirmed` -> `Packed` -> `OutForDelivery` -> `Delivered`
   Alternative terminal states: `Cancelled`, `Returned`.
2. Every state change produces an immutable audit record in `ordering.OrderStatusHistories`.
3. Order transitions emit real-time events through `DeliveryTrackingHub` (SignalR), enabling sub-second UI updates on the Angular frontend.
4. When status moves to `OutForDelivery`, the assigned `DeliveryPartner` streams GPS coordinates every 3-5 seconds, updating the dynamic ETA.

