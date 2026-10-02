# Routing Architecture Specification

This document details the client-side routing tree, layout nesting, lazy loading strategy, page titles, and route guard status in the QuickCart frontend.

---

## 1. Route Tree Definition

The routing configuration is defined in `src/app/app.routes.ts`:

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
        title: 'QuickCart - Instant 10-Minute Grocery Delivery',
      },
      {
        path: 'checkout',
        loadComponent: () =>
          import('./modules/orders/pages/checkout/checkout.component').then(
            (m) => m.CheckoutComponent
          ),
        title: 'QuickCart - Checkout & Delivery',
      },
      {
        path: 'order-success/:id',
        loadComponent: () =>
          import('./modules/orders/pages/order-success/order-success.component').then(
            (m) => m.OrderSuccessComponent
          ),
        title: 'QuickCart - Order Confirmed',
      },
      {
        path: 'order-success',
        loadComponent: () =>
          import('./modules/orders/pages/order-success/order-success.component').then(
            (m) => m.OrderSuccessComponent
          ),
        title: 'QuickCart - Order Confirmed',
      },
    ],
  },
  {
    path: 'login',
    loadComponent: () =>
      import('./modules/identity/pages/login/login.component').then((m) => m.LoginComponent),
    title: 'QuickCart - Log in or Sign up',
  },
  {
    path: 'login/verify',
    loadComponent: () =>
      import('./modules/identity/pages/otp-verify/otp-verify.component').then(
        (m) => m.OtpVerifyComponent
      ),
    title: 'QuickCart - Verify OTP',
  },
  {
    path: 'login/password',
    loadComponent: () =>
      import('./modules/identity/pages/password-login/password-login.component').then(
        (m) => m.PasswordLoginComponent
      ),
    title: 'QuickCart - Email & Password Sign In',
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./modules/identity/pages/register/register.component').then(
        (m) => m.RegisterComponent
      ),
    title: 'QuickCart - Create Account',
  },
  {
    path: '**',
    redirectTo: '',
  },
];
```

---

## 2. Layout Structure & Nested Routes

### Main Application Layout (`MainLayoutComponent`)
Routes nested under `path: ''` share the persistent commerce frame:
* **Header**: Contains store brand logo, delivery badge, search bar, theme toggle, profile menu, and cart trigger.
* **Main Container**: Renders child components inside `<router-outlet />` with responsive padding and maximum width constraint (`1280px`).
* **Cart Drawer**: Global slide-out drawer (`<app-cart-drawer />`) accessible across all catalog and checkout screens.
* **Footer**: Standard footer navigation and copyright.

### Full-Screen Authentication Views
Authentication routes (`/login`, `/login/verify`, `/login/password`, `/register`) are unnested top-level routes that render outside `MainLayoutComponent`. They display dedicated centered card layouts wrapped in `AuthCardShellComponent`.

---

## 3. Route Guard Status

* **Guard Implementation**: `authGuard` (`src/app/core/guards/auth.guard.ts`) is fully implemented as a functional `CanActivateFn`.
* **Current Route Attachment**:
  ```text
  Status: Implemented but Unattached to Routes

  Evidence:
  Inspected src/app/app.routes.ts. None of the route definitions currently include `canActivate: [authGuard]`.
  ```
* **Operational Impact**: Users can currently navigate to `/checkout` directly without prior authentication. The checkout component pre-fills user details if authenticated, or allows checkout input anonymously.

---

## 4. Wildcard Handling
* Unknown paths (`**`) redirect automatically to the home page (`redirectTo: ''`).
