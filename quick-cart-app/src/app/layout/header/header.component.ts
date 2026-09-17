import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CartService } from '../../modules/cart/services/cart.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  template: `
    <header class="header">
      <div class="header-container">
        <!-- Logo & Delivery Tag -->
        <div class="brand-section">
          <a routerLink="/" class="logo">
            <span class="logo-highlight">Quick</span>Cart
          </a>
          <div class="delivery-badge">
            <span class="bolt">⚡</span>
            <div class="delivery-text">
              <span class="delivery-time">10-15 MINS</span>
              <span class="delivery-loc">Bengaluru, 560001</span>
            </div>
          </div>
        </div>

        <!-- Search Bar -->
        <div class="search-section">
          <div class="search-box">
            <span class="search-icon">🔍</span>
            <input
              type="text"
              placeholder="Search for groceries, snacks, beverages..."
              [(ngModel)]="searchQuery"
              (keyup.enter)="onSearch()"
            />
          </div>
        </div>

        <!-- Right Action Buttons (Account & Cart) -->
        <div class="action-section">
          @if (auth.currentUser(); as user) {
            <div class="user-menu">
              <span class="user-name">👤 {{ user.fullName }}</span>
              <button (click)="auth.logout()" class="btn-text">Logout</button>
            </div>
          } @else {
            <a routerLink="/login" class="btn-account">
              <span class="account-icon">👤</span>
              <span>Sign In</span>
            </a>
          }

          <!-- Cart Floating Pill Button -->
          <button class="btn-cart" (click)="cart.toggleDrawer()">
            <span class="cart-icon">🛒</span>
            <div class="cart-info">
              <span class="cart-count">{{ cart.totalItems() }} items</span>
              <span class="cart-total">₹{{ cart.subtotal() | number:'1.2-2' }}</span>
            </div>
          </button>
        </div>
      </div>
    </header>
  `,
  styles: [`
    .header {
      background: #ffffff;
      box-shadow: 0 2px 10px rgba(0, 0, 0, 0.06);
      position: sticky;
      top: 0;
      z-index: 100;
    }
    .header-container {
      max-width: 1280px;
      margin: 0 auto;
      padding: 0.75rem 1.5rem;
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 1.5rem;
    }
    .brand-section {
      display: flex;
      align-items: center;
      gap: 1.25rem;
    }
    .logo {
      font-size: 1.6rem;
      font-weight: 800;
      color: #1a1a1a;
      text-decoration: none;
      letter-spacing: -0.5px;
    }
    .logo-highlight {
      color: #10b981;
    }
    .delivery-badge {
      display: flex;
      align-items: center;
      background: #f0fdf4;
      border: 1px solid #bbf7d0;
      padding: 0.25rem 0.6rem;
      border-radius: 8px;
      gap: 0.4rem;
    }
    .bolt {
      font-size: 1rem;
    }
    .delivery-text {
      display: flex;
      flex-direction: column;
      font-size: 0.72rem;
      line-height: 1.1;
    }
    .delivery-time {
      font-weight: 800;
      color: #047857;
    }
    .delivery-loc {
      color: #6b7280;
    }
    .search-section {
      flex: 1;
      max-width: 580px;
    }
    .search-box {
      display: flex;
      align-items: center;
      background: #f3f4f6;
      border: 1px solid #e5e7eb;
      border-radius: 12px;
      padding: 0.5rem 1rem;
      transition: all 0.2s ease;
    }
    .search-box:focus-within {
      background: #ffffff;
      border-color: #10b981;
      box-shadow: 0 0 0 3px rgba(16, 185, 129, 0.15);
    }
    .search-icon {
      font-size: 0.9rem;
      margin-right: 0.5rem;
      opacity: 0.6;
    }
    .search-box input {
      border: none;
      background: transparent;
      outline: none;
      width: 100%;
      font-size: 0.95rem;
      color: #111827;
    }
    .action-section {
      display: flex;
      align-items: center;
      gap: 1rem;
    }
    .btn-account {
      display: flex;
      align-items: center;
      gap: 0.4rem;
      color: #374151;
      text-decoration: none;
      font-weight: 600;
      font-size: 0.95rem;
      padding: 0.5rem 0.8rem;
      border-radius: 8px;
    }
    .btn-account:hover {
      background: #f9fafb;
      color: #10b981;
    }
    .user-menu {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      font-size: 0.9rem;
      font-weight: 600;
    }
    .btn-text {
      background: none;
      border: none;
      color: #ef4444;
      cursor: pointer;
      font-size: 0.85rem;
      font-weight: 600;
    }
    .btn-cart {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      background: #10b981;
      color: #ffffff;
      border: none;
      padding: 0.6rem 1.1rem;
      border-radius: 12px;
      font-weight: 700;
      cursor: pointer;
      box-shadow: 0 4px 12px rgba(16, 185, 129, 0.3);
      transition: transform 0.15s ease, background 0.2s ease;
    }
    .btn-cart:hover {
      background: #059669;
      transform: translateY(-1px);
    }
    .cart-icon {
      font-size: 1.25rem;
    }
    .cart-info {
      display: flex;
      flex-direction: column;
      text-align: left;
      line-height: 1.15;
    }
    .cart-count {
      font-size: 0.75rem;
      opacity: 0.9;
    }
    .cart-total {
      font-size: 0.95rem;
      font-weight: 800;
    }
  `]
})
export class HeaderComponent {
  readonly cart = inject(CartService);
  readonly auth = inject(AuthService);
  searchQuery = '';

  onSearch(): void {
    if (this.searchQuery.trim()) {
      console.log('Searching for:', this.searchQuery);
    }
  }
}
