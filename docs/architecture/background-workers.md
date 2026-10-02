# Background Workers & Asynchronous Task Processing

This document evaluates the background worker architecture, asynchronous job runners, scheduled cron tasks, and outbox processors across the QuickCart backend.

---

## 1. Architectural Status

```text
Status: Not Implemented / Not Confirmed

Evidence:
1. Inspected all C# source files under src/ (BuildingBlocks, Modules, QuickCart.Api) for IHostedService, BackgroundService, or System.Threading.Channels queue workers. None exist.
2. Inspected all 46 project files (.csproj) for background job libraries (such as Hangfire, Quartz.NET, Coravel, or Azure Functions). None are referenced.
3. Inspected src/QuickCart.Api/Bootstrap/DependencyInjection.cs. No services.AddHostedService<T>() registrations are configured.
```

---

## 2. Current Request Execution Model

In the current codebase, all business operations execute **in-process and synchronously within the HTTP request pipeline**:
* **Order Placement**: `OrderService.CreateOrderAsync` validates cart items, persists the order, and appends status history within the client's HTTP POST request.
* **Payment Webhooks**: `PaymentService.ProcessWebhookAsync` verifies the HMAC signature, updates `payment.Payments`, and invokes `IOrderService.UpdateOrderStatusAsync` immediately before returning HTTP 200 OK.
* **Delivery Assignment**: `DeliveryService` assigns riders and updates tracking coordinates on demand during API invocations.

---

## 3. Future Architectural Roadmap (When Background Processing is Needed)

As transactional volume scales, several domain operations should be offloaded to dedicated background workers:
1. **Transactional Outbox Pattern**:
   * When an order transitions to `Confirmed`, write an outbox event to PostgreSQL. A `BackgroundService` polls or consumes outbox events to dispatch asynchronous notifications without blocking HTTP checkout threads.
2. **Automated Rider Dispatch & Reassignment**:
   * A background worker periodically monitors unassigned orders in `Placed`/`Confirmed` status and matches them with available `DeliveryPartner` agents based on real-time distance.
3. **Cart Abandonment & Stock Expiration**:
   * A scheduled worker periodically scans ephemeral carts in `cart.Carts` older than a configured threshold to release reserved stock.

