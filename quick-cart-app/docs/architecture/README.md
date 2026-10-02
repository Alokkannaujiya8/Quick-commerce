# QuickCart Frontend — Architecture Documentation Index

Welcome to the comprehensive architecture documentation for the QuickCart frontend client application.

This section provides in-depth technical specifications of the client-side system architecture, design patterns, integration points, and component lifecycles.

---

## Architecture Specifications

| Document | Focus Area | Description |
| :--- | :--- | :--- |
| [application-structure.md](application-structure.md) | File & Folder Anatomy | Physical repository layout, naming conventions, and module structure. |
| [components.md](components.md) | Component Inventory | Standalone components, OnPush detection, presenter vs. container designs. |
| [services.md](services.md) | Service Boundaries | Injectable services, singleton scopes, and responsibilities. |
| [routing.md](routing.md) | Routing & Navigation | Route definitions, layout hierarchy, lazy-loading chunks, and page titles. |
| [authentication-authorization.md](authentication-authorization.md) | Auth Workflows | OTP verification, email/password login, Google GIS integration, tokens. |
| [http-api.md](http-api.md) | HTTP & API Communication | ApiService architecture, backend contracts, and payload schemas. |
| [interceptors.md](interceptors.md) | HTTP Interceptors | Functional `authInterceptor`, header injection, and 401 token refresh loops. |
| [guards.md](guards.md) | Route Guards | Functional `authGuard`, authentication verification, and route protection. |
| [state-management.md](state-management.md) | Reactivity & Stores | Angular Signals, computed derivations, effects, and RxJS pipelines. |
| [forms-validation.md](forms-validation.md) | Forms & Validation | Template-driven forms, phone/OTP sanitization, and alert feedback. |
| [error-handling.md](error-handling.md) | Error Architecture | RFC 7807 problem details parsing, service fallbacks, and UI errors. |
| [testing.md](testing.md) | Unit Testing Architecture | Karma, Jasmine, HttpTestingController, test results, and spec inventory. |
| [build-deployment.md](build-deployment.md) | Build & Distribution | `@angular/build:application` builder, budgets, and production outputs. |
| [diagrams/README.md](diagrams/README.md) | Architecture Diagrams | Mermaid diagrams illustrating auth, checkout, cart state, and layout shells. |

---

## Operating Directives

All architecture documentation in this directory is grounded exclusively in verified evidence from the current codebase.

For features not currently confirmed or implemented in the frontend code, specifications adhere strictly to the format:
```text
Status: Not Implemented / Not Confirmed

Evidence:
<what was inspected>
```
