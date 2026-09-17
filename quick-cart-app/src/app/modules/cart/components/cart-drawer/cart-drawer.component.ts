import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { CartService } from '../../services/cart.service';

@Component({
  selector: 'app-cart-drawer',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (cart.isDrawerOpen()) {
      <!-- Backdrop -->
      <div class="cart-backdrop" (click)="cart.closeDrawer()"></div>

      <!-- Slide-in Drawer -->
      <aside class="cart-drawer">
        <div class="drawer-header">
          <h3>My Cart ({{ cart.totalItems() }})</h3>
          <button class="btn-close" (click)="cart.closeDrawer()">✕</button>
        </div>

        @if (cart.items().length === 0) {
          <div class="empty-cart">
            <span class="empty-icon">🛒</span>
            <h4>Your cart is empty</h4>
            <p>Add items from the store to get them delivered in minutes!</p>
            <button class="btn-browse" (click)="cart.closeDrawer()">Start Shopping</button>
          </div>
        } @else {
          <!-- Cart Items List -->
          <div class="cart-items">
            @for (item of cart.items(); track item.productId) {
              <div class="cart-item">
                <div class="item-thumb">
                  @if (item.imageUrl) {
                    <img [src]="item.imageUrl" [alt]="item.name" />
                  } @else {
                    <div class="placeholder-icon">📦</div>
                  }
                </div>

                <div class="item-details">
                  <span class="item-name">{{ item.name }}</span>
                  <span class="item-unit">{{ item.unitOfMeasure }}</span>
                  <span class="item-price">₹{{ item.price | number:'1.2-2' }}</span>
                </div>

                <!-- Quantity Controls -->
                <div class="qty-control">
                  <button (click)="cart.updateQuantity(item.productId, -1)">-</button>
                  <span class="qty-val">{{ item.quantity }}</span>
                  <button (click)="cart.updateQuantity(item.productId, 1)">+</button>
                </div>
              </div>
            }
          </div>

          <!-- Bill Summary -->
          <div class="drawer-footer">
            <div class="bill-details">
              <div class="bill-row">
                <span>Items Subtotal</span>
                <span>₹{{ cart.subtotal() | number:'1.2-2' }}</span>
              </div>
              <div class="bill-row">
                <span>Delivery Charge</span>
                <span>
                  @if (cart.deliveryFee() === 0) {
                    <span class="free-text">FREE</span>
                  } @else {
                    ₹{{ cart.deliveryFee() | number:'1.2-2' }}
                  }
                </span>
              </div>
              <div class="bill-row total-row">
                <span>Grand Total</span>
                <span class="total-amount">₹{{ cart.totalAmount() | number:'1.2-2' }}</span>
              </div>
            </div>

            <button class="btn-checkout" (click)="goToCheckout()">
              <span>Proceed to Pay</span>
              <span>₹{{ cart.totalAmount() | number:'1.2-2' }} ➔</span>
            </button>
          </div>
        }
      </aside>
    }
  `,
  styles: [`
    .cart-backdrop {
      position: fixed;
      inset: 0;
      background: rgba(0, 0, 0, 0.45);
      backdrop-filter: blur(2px);
      z-index: 200;
    }
    .cart-drawer {
      position: fixed;
      top: 0;
      right: 0;
      bottom: 0;
      width: 100%;
      max-width: 420px;
      background: #ffffff;
      box-shadow: -4px 0 25px rgba(0, 0, 0, 0.15);
      z-index: 210;
      display: flex;
      flex-direction: column;
      animation: slideIn 0.25s ease-out;
    }
    @keyframes slideIn {
      from { transform: translateX(100%); }
      to { transform: translateX(0); }
    }
    .drawer-header {
      padding: 1.25rem 1.5rem;
      display: flex;
      justify-content: space-between;
      align-items: center;
      border-bottom: 1px solid #e5e7eb;
    }
    .drawer-header h3 {
      margin: 0;
      font-size: 1.15rem;
      font-weight: 700;
      color: #111827;
    }
    .btn-close {
      background: #f3f4f6;
      border: none;
      width: 32px;
      height: 32px;
      border-radius: 50%;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: 700;
    }
    .empty-cart {
      flex: 1;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 2rem;
      text-align: center;
    }
    .empty-icon {
      font-size: 3.5rem;
      margin-bottom: 1rem;
      opacity: 0.7;
    }
    .empty-cart h4 {
      margin: 0 0 0.5rem;
      font-size: 1.2rem;
      font-weight: 700;
    }
    .empty-cart p {
      color: #6b7280;
      font-size: 0.9rem;
      margin-bottom: 1.5rem;
    }
    .btn-browse {
      background: #10b981;
      color: #fff;
      border: none;
      padding: 0.6rem 1.2rem;
      border-radius: 8px;
      font-weight: 600;
      cursor: pointer;
    }
    .cart-items {
      flex: 1;
      overflow-y: auto;
      padding: 1rem 1.5rem;
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }
    .cart-item {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      padding-bottom: 0.75rem;
      border-bottom: 1px solid #f3f4f6;
    }
    .item-thumb {
      width: 52px;
      height: 52px;
      background: #f9fafb;
      border: 1px solid #e5e7eb;
      border-radius: 8px;
      display: flex;
      align-items: center;
      justify-content: center;
      overflow: hidden;
    }
    .item-thumb img {
      width: 100%;
      height: 100%;
      object-fit: contain;
    }
    .placeholder-icon {
      font-size: 1.5rem;
    }
    .item-details {
      flex: 1;
      display: flex;
      flex-direction: column;
    }
    .item-name {
      font-size: 0.9rem;
      font-weight: 600;
      color: #1f2937;
    }
    .item-unit {
      font-size: 0.75rem;
      color: #6b7280;
    }
    .item-price {
      font-size: 0.9rem;
      font-weight: 700;
      color: #111827;
      margin-top: 0.2rem;
    }
    .qty-control {
      display: flex;
      align-items: center;
      background: #10b981;
      border-radius: 6px;
      overflow: hidden;
    }
    .qty-control button {
      background: none;
      border: none;
      color: #ffffff;
      padding: 0.25rem 0.55rem;
      font-weight: 700;
      cursor: pointer;
    }
    .qty-val {
      color: #ffffff;
      font-weight: 700;
      font-size: 0.85rem;
      min-width: 20px;
      text-align: center;
    }
    .drawer-footer {
      padding: 1.25rem 1.5rem;
      background: #f9fafb;
      border-top: 1px solid #e5e7eb;
    }
    .bill-details {
      display: flex;
      flex-direction: column;
      gap: 0.4rem;
      margin-bottom: 1rem;
      font-size: 0.85rem;
    }
    .bill-row {
      display: flex;
      justify-content: space-between;
      color: #4b5563;
    }
    .free-text {
      color: #10b981;
      font-weight: 700;
    }
    .total-row {
      font-size: 1rem;
      font-weight: 800;
      color: #111827;
      padding-top: 0.4rem;
      border-top: 1px dashed #d1d5db;
    }
    .total-amount {
      color: #047857;
    }
    .btn-checkout {
      width: 100%;
      background: #10b981;
      color: #ffffff;
      border: none;
      padding: 0.85rem 1rem;
      border-radius: 10px;
      font-weight: 700;
      font-size: 1rem;
      display: flex;
      justify-content: space-between;
      cursor: pointer;
      box-shadow: 0 4px 14px rgba(16, 185, 129, 0.35);
    }
    .btn-checkout:hover {
      background: #059669;
    }
  `]
})
export class CartDrawerComponent {
  readonly cart = inject(CartService);
  private readonly router = inject(Router);

  goToCheckout(): void {
    this.cart.closeDrawer();
    this.router.navigate(['/checkout']);
  }
}
