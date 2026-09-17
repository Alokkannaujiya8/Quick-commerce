# Architecture Decision Record: Hyperlocal Dark Store Serviceability

## Status
Approved

## Context
QuickCart requires delivering customer orders within 10 minutes. Standard e-commerce distribution models based on centralized regional warehouses cannot fulfill this requirement.

## Decision
1. Customer fulfillment will be tied strictly to a network of distributed **Dark Stores** (micro-fulfillment centers) covering a 3–5 km radius.
2. The user's device coordinates (`Latitude`, `Longitude`) are matched against active dark store catchment polygons/radii via `GeoLocationService`.
3. Inventory availability presented in the Catalog must reflect the active stock balance in the customer's assigned dark store (`StoreInventory`), not global stock totals.
4. If a customer is outside all dark store coverage zones, the system indicates an "Unserviceable Location" error before items can be added to the cart.

