# Architecture Decision Records (ADRs)

This directory contains the immutable record of significant architectural decisions made for the QuickCart platform.

---

## ADR Index

| ADR ID | Title | Status | Date | Primary Scope |
| :--- | :--- | :--- | :--- | :--- |
| **[ADR-0001](ADR-0001-hyperlocal-darkstore-serviceability.md)** | Hyperlocal Dark Store Serviceability & Catchment Polygons | Approved | 2026-09-06 | Inventory & Catalog |
| **[ADR-0002](ADR-0002-order-delivery-state-machine.md)** | 10-Minute Order Fulfillment & Delivery State Machine | Approved | 2026-09-07 | Ordering & Delivery |
| **[ADR-0003](ADR-0003-multi-schema-database-isolation.md)** | Multi-Schema PostgreSQL Isolation without Cross-Schema Keys | Approved | 2026-09-07 | Persistence & Database |
| **[ADR-0004](ADR-0004-jwt-refresh-token-authentication.md)** | Dual-Token JWT & Cryptographic Rotating Refresh Tokens | Approved | 2026-09-14 | Identity & Security |
| **[ADR-0005](ADR-0005-hmac-sha256-payment-idempotency.md)** | Server-Verified Payment Intents, HMAC-SHA256 & Idempotency | Approved | 2026-09-27 | Payment & Checkout |

---

## ADR Template

```markdown
# ADR-XXXX: Title

## Status
[Draft | Proposed | Approved | Superseded | Deprecated]

## Context
What is the business context, problem statement, or technical constraint driving this decision?

## Decision
What is the change or architecture design being adopted? Detail technical requirements, invariants, and implementation patterns.

## Consequences
### Positive
* What advantages, guarantees, or performance gains are achieved?

### Negative / Trade-offs
* What operational overhead, constraints, or complexities are introduced?

## Verification & Code References
* Source files, entities, or test suites proving implementation.
```

