# Architecture Decisions Ledger

This document maintains the index of confirmed architectural decisions and connects them to the formal Architecture Decision Records (ADRs) located in `docs/adr/`.

---

## Confirmed Architectural Decisions

### DEC-001: Standalone Component Architecture with Built-in Control Flow
* **Status**: Confirmed & Verified
* **Summary**: Eliminated all `NgModule` dependencies across the application. Implemented standalone components exclusively with `@if`, `@else`, `@for`, and `@switch` control flow.
* **Evidence**: Audited all component files in `src/app/`. Zero `NgModule` declarations found; all components declare `standalone: true`.
* **ADR Link**: [ADR 0001: Standalone Components & Control Flow](../adr/0001-standalone-components-and-control-flow.md)

### DEC-002: Signal-First State Management with RxJS for Asynchronous I/O
* **Status**: Confirmed & Verified
* **Summary**: Standardized on Angular Signals (`signal`, `computed`, `effect`) for synchronous UI and domain store state (`CartService`, `AuthService`, `ThemeService`), rejecting external state management libraries (NgRx) to keep bundles lightweight and responsive. RxJS is reserved for asynchronous network operations.
* **Evidence**: Audited `package.json` (no `@ngrx` packages) and `CartService`, `ThemeService`, `AuthService` source code.
* **ADR Link**: [ADR 0002: Signals & RxJS Hybrid Reactivity](../adr/0002-signals-and-rxjs-hybrid-state.md)

### DEC-003: Functional JWT Interceptor with Silent 401 Token Refresh Rotation
* **Status**: Confirmed & Verified
* **Summary**: Implemented `authInterceptor` as an Angular functional HTTP interceptor (`HttpInterceptorFn`) that transparently attaches JWT Bearer tokens and handles `401 Unauthorized` token renewal via `AuthService.refreshToken()`.
* **Evidence**: Audited `src/app/core/interceptors/auth.interceptor.ts` and `src/app/app.config.ts`.
* **ADR Link**: [ADR 0003: JWT Interception & Refresh Rotation](../adr/0003-jwt-auth-interceptor-with-refresh-rotation.md)

### DEC-004: Native Google Identity Services Integration with Single-Initialization Popup
* **Status**: Confirmed & Verified
* **Summary**: Integrated Google Identity Services without deprecated One Tap callbacks, implementing idempotent single initialization to eliminate console logger warnings and ensure seamless NgZone re-entry.
* **Evidence**: Audited `src/app/core/auth/google-identity.service.ts` and unit tests in `google-identity.service.spec.ts`.
* **ADR Link**: [ADR 0004: Google Identity Services Integration](../adr/0004-google-identity-services-integration.md)
