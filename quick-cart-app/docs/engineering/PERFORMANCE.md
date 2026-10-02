# QuickCart Frontend — Performance & Optimization Guide

This document details verified performance strategies, bundle configurations, change detection optimizations, and caching mechanisms implemented in the QuickCart frontend.

---

## 1. Route-Level Lazy Loading

All feature views in `src/app/app.routes.ts` are loaded on-demand via dynamic imports (`loadComponent`):

```typescript
export const routes: Routes = [
  {
    path: '',
    component: MainLayoutComponent,
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./modules/catalog/pages/product-list/product-list.component').then(
            (m) => m.ProductListComponent
          ),
      },
      {
        path: 'checkout',
        loadComponent: () =>
          import('./modules/orders/pages/checkout/checkout.component').then(
            (m) => m.CheckoutComponent
          ),
      },
      {
        path: 'order-success/:id',
        loadComponent: () =>
          import('./modules/orders/pages/order-success/order-success.component').then(
            (m) => m.OrderSuccessComponent
          ),
      },
    ],
  },
  {
    path: 'login',
    loadComponent: () =>
      import('./modules/identity/pages/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'login/verify',
    loadComponent: () =>
      import('./modules/identity/pages/otp-verify/otp-verify.component').then(
        (m) => m.OtpVerifyComponent
      ),
  },
  {
    path: 'login/password',
    loadComponent: () =>
      import('./modules/identity/pages/password-login/password-login.component').then(
        (m) => m.PasswordLoginComponent
      ),
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./modules/identity/pages/register/register.component').then(
        (m) => m.RegisterComponent
      ),
  },
];
```

### Verified Lazy Chunk Separation
During compilation with `@angular/build:application`, discrete lazy chunk files are generated:
* `checkout-component`: ~17.96 kB (transfer: ~4.43 kB)
* `product-list-component`: ~14.26 kB (transfer: ~4.29 kB)
* `order-success-component`: ~10.48 kB (transfer: ~2.71 kB)
* `login-component`: ~9.80 kB (transfer: ~3.39 kB)
* `register-component`: ~5.10 kB (transfer: ~1.61 kB)
* `otp-verify-component`: ~4.91 kB (transfer: ~1.82 kB)
* `password-login-component`: ~4.01 kB (transfer: ~1.41 kB)

This ensures that authentication and checkout modules are never downloaded until requested by the user.

---

## 2. Change Detection Optimization

### OnPush Strategy
Major views and shared presenters employ `changeDetection: ChangeDetectionStrategy.OnPush`:
* `CheckoutComponent`
* `OrderSuccessComponent`
* `LoginComponent`
* `OtpVerifyComponent`
* `PasswordLoginComponent`
* `RegisterComponent`
* `HeaderComponent`

With `OnPush`, Angular skips checking the component and its subtree unless:
1. An `@Input()` reference changes.
2. An event handler inside the component fires.
3. An Angular Signal bound in the template updates.
4. An async pipe emits a new value.

### Event Coalescing
Configured in `src/app/app.config.ts`:
```typescript
provideZoneChangeDetection({ eventCoalescing: true })
```
Event coalescing batches multiple asynchronous events occurring within the same tick of the JavaScript event loop into a single change detection pass, drastically reducing template re-evaluation overhead.

---

## 3. Angular Signals Reactivity

* Signals (`signal()`, `computed()`) allow fine-grained reactivity.
* In `CartService`, derived metrics (`totalItems`, `subtotal`, `deliveryFee`, `totalAmount`) are computed lazily and memoized. They only recompute when the underlying `items()` signal changes.
* This eliminates repetitive manual array reductions on every change detection cycle.

---

## 4. Bundle Optimization & Budgets

Configured in `angular.json` for production builds:
```json
"budgets": [
  {
    "type": "initial",
    "maximumWarning": "500kB",
    "maximumError": "1MB"
  },
  {
    "type": "anyComponentStyle",
    "maximumWarning": "8kB",
    "maximumError": "16kB"
  }
]
```
* **Initial Transfer Size**: Verified at ~98.19 kB (compressed), well below the 500 kB budget threshold.
* **Component Style Budget**: Kept lean by relying on shared SCSS variables and lightweight component styling.

---

## 5. Asset & Image Optimization

* Product images in `CatalogService` are served with query parameters optimizing quality and dimensions (e.g. `unsplash.com/...&auto=format&fit=crop&w=400&q=80`).
* Icons and logos (`BrandLogoComponent`) use native inline SVG or lightweight Unicode emojis, requiring zero HTTP network requests for icons.

---

## 6. Caching & State Persistence

* **Local Storage Caching**:
  * Shopping cart state (`qc_cart_items`) is stored in `localStorage` so user selections persist across browser refreshes without repeated server round-trips.
  * Theme preference (`qc_theme`) is stored in `localStorage` and applied immediately before initial render to avoid layout shifts or theme flashing.
  * User profile (`qc_user_profile`) is cached locally to render the header greeting instantly.
