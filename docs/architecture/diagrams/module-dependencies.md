# Module Dependencies & Clean Architecture Layering

This diagram illustrates the inward dependency flow of Clean Architecture across the 8 business modules, the shared BuildingBlocks foundation, and cross-module contract interactions.

---

```mermaid
flowchart TD
    subgraph Host["Composition Host"]
        API["src/QuickCart.Api"]
    end

    subgraph ModuleArch["Clean Architecture per Module (Example: Payment)"]
        Domain["Payment.Domain<br/>(Entities, Invariants, Enums)"]
        Application["Payment.Application<br/>(Use Cases, DTOs, Interfaces)"]
        Infrastructure["Payment.Infrastructure<br/>(DbContext, Repositories, Gateway)"]
        Presentation["Payment.Presentation<br/>(PaymentsController)"]
        
        Presentation --> Application
        Presentation --> Domain
        Infrastructure --> Application
        Infrastructure --> Domain
        Application --> Domain
    end

    subgraph SharedBlocks["BuildingBlocks (src/BuildingBlocks)"]
        BBDomain["BuildingBlocks.Domain<br/>(Entity, AuditableEntity, IDomainEvent)"]
        BBApp["BuildingBlocks.Application<br/>(ICommand, IQuery, Result, PagedResult)"]
        BBInfra["BuildingBlocks.Infrastructure<br/>(AuditableEntityInterceptor)"]
        BBPres["BuildingBlocks.Presentation<br/>(GlobalExceptionHandler, ApiResponse)"]

        Domain --> BBDomain
        Application --> BBApp
        Infrastructure --> BBInfra
        Presentation --> BBPres
    end

    subgraph CrossModule["Cross-Module Contract Invocations"]
        OrderingApp["Ordering.Application<br/>(IOrderService, OrderDto)"]
        Application -->|Consumes Contract| OrderingApp
    end

    API --> Presentation
    API --> Infrastructure
```

