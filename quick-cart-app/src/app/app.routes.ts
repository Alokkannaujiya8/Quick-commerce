import { Routes } from '@angular/router';
import { MainLayoutComponent } from './layout/main-layout/main-layout.component';

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
