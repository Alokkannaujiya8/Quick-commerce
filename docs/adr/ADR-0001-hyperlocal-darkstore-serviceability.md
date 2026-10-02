# ADR-0001: Hyperlocal Dark Store Serviceability & Catchment Polygons

## Status
Approved

## Context
QuickCart operates under strict temporal constraints: delivering groceries and FMCG goods within 10 minutes of order placement. Standard centralized e-commerce distribution models relying on regional distribution hubs cannot meet this sub-minute fulfillment SLA.

## Decision
1. **Micro-Fulfillment Centers**: Fulfillment is partitioned across a distributed network of localized Dark Stores, each serving a strict 3–5 km radius catchment polygon.
2. **GPS Serviceability Matching**: The customer's device coordinates (`Latitude`, `Longitude`) are matched against active dark stores via `GeoLocationService` (`POST /api/serviceability`).
3. **Localized Catalog Balances**: Product availability and quantities presented to the customer are scoped to the assigned dark store (`inventory.StoreInventories`), not a global inventory total.
4. **Out-of-Service Barrier**: Customers outside active dark store coverage zones are marked unserviceable before items can be added to cart.

## Consequences
### Positive
* Enables sub-minute order picking and dispatch.
* Eliminates out-of-stock cancellations by displaying only localized physical inventory.
* Reduces rider travel distances, guaranteeing the 10-minute delivery promise.

### Negative / Trade-offs
* Requires real-time synchronization between store-level warehouse management systems and `inventory.StoreInventories`.
* Requires spatial math calculations per serviceability check.

## Verification & Code References
* Entities: [`DarkStore`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/Modules/Inventory/Inventory.Domain/Entities/DarkStore.cs), [`StoreInventory`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/Modules/Inventory/Inventory.Domain/Entities/StoreInventory.cs)
* Service: [`GeoLocationService`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/Modules/Inventory/Inventory.Infrastructure/Services/GeoLocationService.cs)
* Controller: [`DarkStoresController`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/Modules/Inventory/Inventory.Presentation/Controllers/DarkStoresController.cs)

