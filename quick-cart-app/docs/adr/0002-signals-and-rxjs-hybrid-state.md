# ADR 0002: Hybrid Reactivity: Angular Signals with RxJS Streams

## Status
**Accepted**

## Context
Complex single-page applications often introduce external state management libraries such as NgRx, NGXS, or Akita. For a quick-commerce application, state consists of:
1. Synchronous UI and domain state (active cart items, totals, drawer state, active category chip, dark theme).
2. Asynchronous network I/O (product catalog retrieval, OTP verification, order placement, payment intent creation).

Introducing a heavyweight Redux-style store introduces significant cognitive overhead, action boilerplate, and bundle bloat without proportional performance gains.

## Decision
Adopt a hybrid reactivity architecture:
1. **Angular Signals (`signal()`, `computed()`, `effect()`)** for all synchronous component and shared domain store state (`CartService`, `AuthService`, `ThemeService`).
2. **RxJS Observables (`HttpClient`, `switchMap`, `catchError`)** for handling asynchronous HTTP requests and multi-step async orchestration.
3. Synchronize HTTP responses into Signals inside `.pipe(tap(...))` or subscription callbacks.

## Consequences
### Positive
* Zero additional external dependencies.
* Glitch-free, synchronous derived metrics (`totalItems`, `subtotal`, `deliveryFee`) memoized automatically.
* Perfect interoperability with `OnPush` change detection and zone coalescing.
* Drastically simpler developer experience.

### Negative / Trade-offs
* Developers must understand the boundary: use Signals for state, use RxJS for asynchronous events and streams.
