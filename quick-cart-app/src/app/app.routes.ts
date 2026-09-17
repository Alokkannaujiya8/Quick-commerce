import { Routes } from '@angular/router';
import { MainLayoutComponent } from './layout/main-layout/main-layout.component';
import { ProductListComponent } from './modules/catalog/pages/product-list/product-list.component';
import { CheckoutComponent } from './modules/orders/pages/checkout/checkout.component';
import { OrderSuccessComponent } from './modules/orders/pages/order-success/order-success.component';
import { LoginComponent } from './modules/identity/pages/login.component';
import { RegisterComponent } from './modules/identity/pages/register.component';

export const routes: Routes = [
  {
    path: '',
    component: MainLayoutComponent,
    children: [
      {
        path: '',
        component: ProductListComponent,
        title: 'QuickCart - Instant 10-Minute Grocery Delivery'
      },
      {
        path: 'checkout',
        component: CheckoutComponent,
        title: 'QuickCart - Checkout & Delivery'
      },
      {
        path: 'order-success/:id',
        component: OrderSuccessComponent,
        title: 'QuickCart - Order Confirmed'
      },
      {
        path: 'order-success',
        component: OrderSuccessComponent,
        title: 'QuickCart - Order Confirmed'
      }
    ]
  },
  {
    path: 'login',
    component: LoginComponent,
    title: 'QuickCart - Sign In'
  },
  {
    path: 'register',
    component: RegisterComponent,
    title: 'QuickCart - Create Account'
  },
  {
    path: '**',
    redirectTo: ''
  }
];
