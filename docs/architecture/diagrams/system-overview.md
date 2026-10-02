# System Overview Architecture Diagram

This diagram visualizes the overall architecture of QuickCart: the client tier, the unified ASP.NET Core API host, the 8 autonomous domain modules, shared building blocks, and the multi-schema PostgreSQL persistence layer.

---

```mermaid
flowchart TD
    subgraph Clients["Client Tier"]
        SPA["Angular 20 SPA (quick-cart-app)"]
    end

    subgraph Host["Composition Root (src/QuickCart.Api)"]
        API["ASP.NET Core 10 Web API Host"]
        PIPE["Middleware Pipeline (CORS, Auth, ProblemDetails, Scalar)"]
        HUB["SignalR Hub (/hubs/delivery-tracking)"]
    end

    subgraph Modules["Domain Modules (src/Modules)"]
        Catalog["Catalog Module"]
        Inventory["Inventory Module"]
        Ordering["Ordering Module"]
        Cart["Cart Module"]
        Delivery["Delivery Module"]
        Identity["Identity Module"]
        Payment["Payment Module"]
        Promotion["Promotion Module"]
    end

    subgraph Foundation["Shared Primitives (src/BuildingBlocks)"]
        BBDomain["BuildingBlocks.Domain"]
        BBApp["BuildingBlocks.Application"]
        BBInfra["BuildingBlocks.Infrastructure"]
        BBPresent["BuildingBlocks.Presentation"]
    end

    subgraph Storage["PostgreSQL 16+ Database (quickcartdb)"]
        DBCatalog[("catalog schema")]
        DBInventory[("inventory schema")]
        DBOrdering[("ordering schema")]
        DBCart[("cart schema")]
        DBDelivery[("delivery schema")]
        DBIdentity[("identity schema")]
        DBPayment[("payment schema")]
        DBPromotion[("promotion schema")]
    end

    SPA -->|HTTPS / REST API| API
    SPA -->|WebSockets| HUB
    API --> PIPE
    API --> Modules
    Modules --> Foundation

    Catalog --> DBCatalog
    Inventory --> DBInventory
    Ordering --> DBOrdering
    Cart --> DBCart
    Delivery --> DBDelivery
    Identity --> DBIdentity
    Payment --> DBPayment
    Promotion --> DBPromotion
```

