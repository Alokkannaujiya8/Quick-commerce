# QuickCart Architecture Documentation

Welcome to the architectural specifications and design documentation for **QuickCart**, a high-throughput 10-minute instant delivery quick commerce platform.

---

## 1. Documentation Index

### Foundational Architecture
* **[Solution Structure](solution-structure.md)**: Physical organization of .NET 10 solution, projects, shared building blocks, and module layers.
* **[API Layer](api.md)**: ASP.NET Core host, routing, Minimal API endpoints, Scalar OpenAPI documentation, and SignalR hub endpoints.
* **[Application Layer](application.md)**: CQRS abstractions, use-case contracts, DTO definitions, and the functional Result pattern.
* **[Domain Layer](domain.md)**: Aggregates, domain invariants, entity base classes, and domain events.
* **[Infrastructure Layer](infrastructure.md)**: EF Core persistence, PostgreSQL schema mappings, interceptors, and external clients.
* **[Database Architecture](database.md)**: Multi-schema PostgreSQL isolation, table definitions, indexing, and migration pipelines.

### Module & Feature Architecture
* **[Domain Modules](modules.md)**: Detailed breakdown and verified implementation status across all 8 modules (Catalog, Inventory, Ordering, Cart, Delivery, Identity, Payment, Promotion).
* **[Authentication & Authorization](authentication-authorization.md)**: JWT access tokens, rotating refresh tokens, Google Sign-In, and BCrypt password security.
* **[Integrations](integrations.md)**: External third-party integrations (Google OAuth, Payment gateways, SignalR WebSockets).

### Non-Functional & Operational Architecture
* **[Multi-Tenancy](multi-tenancy.md)**: Evaluation of tenancy architecture (*Status: Not Implemented / Single-Tenant*).
* **[Caching](caching.md)**: Evaluation of caching architecture (*Status: Not Implemented / Client-Only*).
* **[Background Workers](background-workers.md)**: Evaluation of asynchronous task runners (*Status: Not Implemented / In-Process*).
* **[Deployment](deployment.md)**: Evaluation of deployment tooling and scripts (*Status: Not Implemented / Local Only*).

### Visual Topologies & Diagrams
* **[Architecture Diagrams](diagrams/)**:
  * [System Overview Diagram](diagrams/system-overview.md)
  * [Request Flow Diagram](diagrams/request-flow.md)
  * [Module Dependencies Diagram](diagrams/module-dependencies.md)

### Architectural Decision Records (ADRs)
* **[ADR Index](../adr/README.md)**: Architecture Decision Records catalog.
* **[Hyperlocal Dark Store Serviceability](hyperlocal-darkstore-serviceability.md)**: Fulfillment radius matching and localized catalog availability.
* **[10-Minute Order & Delivery State Machine](order-delivery-state-machine.md)**: Order lifecycle state transitions and real-time telemetry.

---

## 2. Architectural Pillars

1. **Modular Monolith**: Maximizes developer velocity and domain autonomy without the operational complexity or network latency of microservices.
2. **Schema Isolation**: Each business module exclusively owns a dedicated PostgreSQL schema (`catalog`, `inventory`, `ordering`, etc.) with zero cross-schema foreign keys.
3. **Inward Clean Architecture**: Inward dependency flow guarantees business rules in `Domain` and `Application` remain completely decoupled from third-party infrastructure.
4. **Real-Time Telemetry**: Real-time order progress and rider GPS streams powered by SignalR WebSockets.

