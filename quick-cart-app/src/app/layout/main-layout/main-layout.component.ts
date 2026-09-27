import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from '../header/header.component';
import { FooterComponent } from '../footer/footer.component';
import { CartDrawerComponent } from '../../modules/cart/components/cart-drawer/cart-drawer.component';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [RouterOutlet, HeaderComponent, FooterComponent, CartDrawerComponent],
  template: `
    <div class="app-shell">
      <app-header />
      <main class="main-container">
        <router-outlet />
      </main>
      <app-cart-drawer />
      <app-footer />
    </div>
  `,
  styles: [`
    .app-shell {
      min-height: 100vh;
      display: flex;
      flex-direction: column;
      background: #f9fafb;
    }
    .main-container {
      flex: 1;
      max-width: 1280px;
      width: 100%;
      margin: 0 auto;
      padding: 1.5rem 1.5rem;
    }
  `]
})
export class MainLayoutComponent {}
