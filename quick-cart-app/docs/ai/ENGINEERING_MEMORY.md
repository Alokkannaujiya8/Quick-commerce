# Frontend Engineering Memory & Durable Conventions

This document captures institutional knowledge, non-obvious design choices, debugging lessons, and invariant conventions established in the QuickCart frontend.

---

## 1. Core Architectural Invariants

* **Framework Invariant**: Never introduce `NgModule` into this project. Standalone components (`standalone: true`) are the permanent architectural standard.
* **Control Flow Invariant**: Always write templates using modern built-in control flow:
  * `@if (condition) { ... } @else { ... }`
  * `@for (item of items; track item.id) { ... }`
  * Never introduce legacy `*ngIf`, `*ngFor`, or `*ngSwitch`.
* **Change Detection Invariant**: Always declare `changeDetection: ChangeDetectionStrategy.OnPush` on all components.
* **Dependency Injection Invariant**: Prefer the functional `inject(...)` syntax over constructor parameter injection for cleaner inheritance and lifecycle ergonomics.

---

## 2. Reactivity & State Patterns

* **Signals vs. RxJS Boundary**:
  * Use **Angular Signals** for synchronous component and service state (UI toggles, selected filters, cart items, computed totals, user session).
  * Use **RxJS** for asynchronous network requests (`HttpClient`) and stream composition.
  * Update signals inside RxJS `.pipe(tap(...))` or `.subscribe({ next: ... })` to bridge the two reactivity systems seamlessly.
* **Derived Metrics**: Never compute totals, counts, or fees imperatively across multiple methods. Always declare them as `computed(() => ...)` so they are computed lazily and memoized automatically.
* **Side-Effect Management**: Only use `effect()` when synchronizing state to external imperative APIs (such as DOM attributes or `localStorage`).

---

## 3. Google Identity Services (GIS) Caveats

* **Duplicate Initialization Warning**: Calling `google.accounts.id.initialize()` more than once per browser page lifecycle triggers `[GSI_LOGGER]: google.accounts.id.initialize() is called multiple times.` in the developer console. `GoogleIdentityService` prevents this via an internal `initialized = true` flag.
* **Deprecated One Tap Status Callbacks**: Avoid calling deprecated One Tap UI notification methods (`isNotDisplayed`, `isSkippedMoment`, `isDismissedMoment`) as they cause deprecation warnings on modern browsers with third-party cookie restrictions.
* **NgZone Re-Entry**: Callbacks dispatched by the Google GIS library originate outside the Angular execution zone. They must be wrapped in `this.ngZone.run(() => ...)` to trigger Angular change detection immediately.

---

## 4. HTTP Interception & Token Lifecycle

* **Endpoint Exclusion Rule**: Any new authentication endpoints added to the backend (e.g. password resets, magic links) must be added to the exclusion check in `authInterceptor` to prevent recursive 401 token refresh attempts.
* **RFC 7807 Parsing**: Always check `err?.error?.detail` before `err?.error?.message` when rendering error toasts from backend HTTP responses.
