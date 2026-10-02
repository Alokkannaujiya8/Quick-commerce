# Route Guards Architecture Specification

This document details the functional route guard implementations, navigation redirection logic, and current attachment status in the QuickCart frontend.

---

## 1. Functional Guard Paradigm

Angular 20 favors **functional route guards** (`CanActivateFn`, `CanMatchFn`) over legacy class-based `CanActivate` interfaces.

---

## 2. `authGuard` Implementation Details

The guard is located at `src/app/core/guards/auth.guard.ts`:

```typescript
import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree(['/login']);
};
```

### 2.1 Dependency Injection
The guard resolves dependencies in its functional execution context using:
* `inject(AuthService)`: Accesses the reactive identity state.
* `inject(Router)`: Allows creation of redirect navigation targets.

### 2.2 Evaluation Logic
* Evaluates `authService.isAuthenticated()`.
* `isAuthenticated` is an Angular `computed()` signal in `AuthService`:
  ```typescript
  readonly isAuthenticated = computed(() => !!this.currentUser() || !!this.getAccessToken());
  ```
* If either `currentUser` is populated or a valid access token exists in `localStorage`, returns `true`.

### 2.3 Navigation Redirection via `UrlTree`
* If unauthenticated, the guard returns `router.createUrlTree(['/login'])`.
* Returning a `UrlTree` is the recommended Angular practice because it cleanly cancels the current navigation and starts navigation to the target route without leaving orphan router events or history pollution.

---

## 3. Current Guard Deployment & Route Attachment Status

```text
Status: Guard Fully Implemented and Unit Tested; Not Yet Attached in app.routes.ts

Evidence:
Inspected src/app/app.routes.ts:
- Path 'checkout' contains no `canActivate: [authGuard]`.
- Path 'order-success/:id' contains no `canActivate: [authGuard]`.
- Path '' (home) is deliberately public.
```

### Architectural Recommendation
To enforce customer authentication prior to order submission, attach `authGuard` to the `/checkout` route definition:
```typescript
{
  path: 'checkout',
  canActivate: [authGuard],
  loadComponent: () => import('./modules/orders/pages/checkout/checkout.component').then(m => m.CheckoutComponent),
  title: 'QuickCart - Checkout & Delivery'
}
```
*(Note: Per AI non-destructive rules, this has not been modified automatically).*
