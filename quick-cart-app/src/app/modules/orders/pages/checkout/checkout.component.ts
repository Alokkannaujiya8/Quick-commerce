import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CartService } from '../../../cart/services/cart.service';
import { OrderService } from '../../services/order.service';
import { AuthService } from '../../../../core/services/auth.service';
import { CreateOrderRequest, PaymentMethod, ShippingAddress } from '../../models/order.model';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="checkout-page">
      <div class="checkout-header">
        <a routerLink="/" class="back-link">← Back to Store</a>
        <h2>Checkout & Instant Delivery</h2>
        <div class="express-pill">⚡ Guaranteed Delivery in 10 mins</div>
      </div>

      @if (cart.items().length === 0) {
        <div class="empty-state">
          <div class="icon">🛒</div>
          <h3>Your cart is empty</h3>
          <p>Please add products to your cart before proceeding to checkout.</p>
          <a routerLink="/" class="btn-primary">Browse Catalog</a>
        </div>
      } @else {
        <div class="checkout-grid">
          <!-- Left Column: Delivery & Payment Details -->
          <div class="checkout-form-col">
            <!-- 1. Delivery Address Section -->
            <section class="checkout-card">
              <div class="card-title">
                <span class="step-num">1</span>
                <h3>Delivery Address</h3>
              </div>

              <div class="form-body">
                <div class="form-row">
                  <div class="form-group">
                    <label for="fullName">Recipient Name</label>
                    <input
                      id="fullName"
                      type="text"
                      [(ngModel)]="address.fullName"
                      placeholder="e.g. Rahul Sharma"
                      required
                    />
                  </div>
                  <div class="form-group">
                    <label for="phone">Phone Number</label>
                    <input
                      id="phone"
                      type="tel"
                      [(ngModel)]="address.phoneNumber"
                      placeholder="e.g. +91 98765 43210"
                      required
                    />
                  </div>
                </div>

                <div class="form-group">
                  <label for="street">Flat / House No. / Street Address</label>
                  <input
                    id="street"
                    type="text"
                    [(ngModel)]="address.streetAddress"
                    placeholder="Flat 402, Sunshine Heights, 12th Main Road"
                    required
                  />
                </div>

                <div class="form-row">
                  <div class="form-group">
                    <label for="city">City</label>
                    <input
                      id="city"
                      type="text"
                      [(ngModel)]="address.city"
                      placeholder="Bengaluru"
                      required
                    />
                  </div>
                  <div class="form-group">
                    <label for="pincode">Postal / Pincode</label>
                    <input
                      id="pincode"
                      type="text"
                      [(ngModel)]="address.postalCode"
                      placeholder="560001"
                      required
                    />
                  </div>
                </div>

                <div class="address-type-selector">
                  <label>Address Type:</label>
                  <div class="type-pills">
                    <button
                      type="button"
                      [class.active]="address.addressType === 'Home'"
                      (click)="address.addressType = 'Home'"
                    >
                      🏠 Home
                    </button>
                    <button
                      type="button"
                      [class.active]="address.addressType === 'Work'"
                      (click)="address.addressType = 'Work'"
                    >
                      🏢 Work
                    </button>
                    <button
                      type="button"
                      [class.active]="address.addressType === 'Other'"
                      (click)="address.addressType = 'Other'"
                    >
                      📍 Other
                    </button>
                  </div>
                </div>
              </div>
            </section>

            <!-- 2. Payment Method Section -->
            <section class="checkout-card">
              <div class="card-title">
                <span class="step-num">2</span>
                <h3>Payment Method</h3>
              </div>

              <div class="payment-options">
                <label
                  class="payment-option"
                  [class.selected]="selectedPayment === 'COD'"
                >
                  <input
                    type="radio"
                    name="payment"
                    value="COD"
                    [(ngModel)]="selectedPayment"
                  />
                  <div class="option-content">
                    <span class="option-icon">💵</span>
                    <div>
                      <strong>Cash on Delivery / UPI on Delivery</strong>
                      <p>Pay with cash or QR code scan when your order arrives at your door</p>
                    </div>
                  </div>
                </label>

                <label
                  class="payment-option"
                  [class.selected]="selectedPayment === 'UPI'"
                >
                  <input
                    type="radio"
                    name="payment"
                    value="UPI"
                    [(ngModel)]="selectedPayment"
                  />
                  <div class="option-content">
                    <span class="option-icon">⚡</span>
                    <div>
                      <strong>Instant UPI</strong>
                      <p>Google Pay, PhonePe, Paytm, or any UPI App</p>
                    </div>
                  </div>
                </label>

                <label
                  class="payment-option"
                  [class.selected]="selectedPayment === 'CARD'"
                >
                  <input
                    type="radio"
                    name="payment"
                    value="CARD"
                    [(ngModel)]="selectedPayment"
                  />
                  <div class="option-content">
                    <span class="option-icon">💳</span>
                    <div>
                      <strong>Credit / Debit Card</strong>
                      <p>Visa, MasterCard, RuPay with 3D Secure Verification</p>
                    </div>
                  </div>
                </label>
              </div>
            </section>
          </div>

          <!-- Right Column: Order Summary & Pay Action -->
          <div class="order-summary-col">
            <div class="summary-card">
              <h3>Order Summary</h3>
              <div class="items-preview">
                @for (item of cart.items(); track item.productId) {
                  <div class="summary-item">
                    <div class="item-name-qty">
                      <span class="name">{{ item.name }}</span>
                      <span class="qty">Qty: {{ item.quantity }}</span>
                    </div>
                    <span class="price">₹{{ (item.price * item.quantity) | number:'1.2-2' }}</span>
                  </div>
                }
              </div>

              <div class="bill-breakdown">
                <div class="bill-row">
                  <span>Item Total</span>
                  <span>₹{{ cart.subtotal() | number:'1.2-2' }}</span>
                </div>
                <div class="bill-row">
                  <span>Delivery Charge (10 Mins)</span>
                  <span>
                    @if (cart.deliveryFee() === 0) {
                      <strong class="free-badge">FREE</strong>
                    } @else {
                      ₹{{ cart.deliveryFee() | number:'1.2-2' }}
                    }
                  </span>
                </div>
                <div class="bill-row">
                  <span>Handling & Packaging Fee</span>
                  <span>₹2.00</span>
                </div>
                <div class="bill-row grand-total">
                  <span>To Pay</span>
                  <span class="grand-price">₹{{ (cart.totalAmount() + 2) | number:'1.2-2' }}</span>
                </div>
              </div>

              <button
                class="btn-place-order"
                [disabled]="isSubmitting()"
                (click)="placeOrder()"
              >
                @if (isSubmitting()) {
                  <span>Placing Order...</span>
                } @else {
                  <span>Place Order • ₹{{ (cart.totalAmount() + 2) | number:'1.2-2' }}</span>
                }
              </button>

              <div class="trust-notice">
                <span>🔒 100% Safe & Secure Checkout</span>
              </div>
            </div>
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .checkout-page {
      padding-bottom: 3rem;
    }
    .checkout-header {
      margin-bottom: 1.5rem;
      .back-link {
        display: inline-block;
        color: #4b5563;
        font-weight: 500;
        margin-bottom: 0.5rem;
        text-decoration: none;
        &:hover { color: #10b981; }
      }
      h2 {
        font-size: 1.75rem;
        font-weight: 800;
        color: #111827;
        margin: 0 0 0.4rem 0;
      }
      .express-pill {
        display: inline-flex;
        align-items: center;
        background: #ecfdf5;
        color: #065f46;
        border: 1px solid #a7f3d0;
        font-weight: 700;
        font-size: 0.85rem;
        padding: 0.25rem 0.75rem;
        border-radius: 9999px;
      }
    }
    .empty-state {
      background: #ffffff;
      border: 1px solid #e5e7eb;
      border-radius: 16px;
      padding: 4rem 2rem;
      text-align: center;
      .icon { font-size: 3rem; margin-bottom: 1rem; }
      h3 { font-size: 1.3rem; margin-bottom: 0.5rem; }
      p { color: #6b7280; margin-bottom: 1.5rem; }
      .btn-primary {
        display: inline-block;
        background: #10b981;
        color: #fff;
        font-weight: 600;
        padding: 0.75rem 1.5rem;
        border-radius: 8px;
        text-decoration: none;
      }
    }
    .checkout-grid {
      display: grid;
      grid-template-columns: 1fr 380px;
      gap: 1.5rem;
      align-items: start;
      @media (max-width: 900px) {
        grid-template-columns: 1fr;
      }
    }
    .checkout-card {
      background: #ffffff;
      border: 1px solid #e5e7eb;
      border-radius: 16px;
      padding: 1.5rem;
      margin-bottom: 1.5rem;
      box-shadow: 0 1px 3px rgba(0,0,0,0.05);

      .card-title {
        display: flex;
        align-items: center;
        gap: 0.75rem;
        margin-bottom: 1.25rem;
        padding-bottom: 0.75rem;
        border-bottom: 1px solid #f3f4f6;

        .step-num {
          width: 28px;
          height: 28px;
          border-radius: 50%;
          background: #10b981;
          color: white;
          font-weight: 700;
          display: flex;
          align-items: center;
          justify-content: center;
          font-size: 0.85rem;
        }
        h3 {
          margin: 0;
          font-size: 1.15rem;
          color: #1f2937;
        }
      }
    }
    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1rem;
      @media (max-width: 600px) {
        grid-template-columns: 1fr;
      }
    }
    .form-group {
      margin-bottom: 1rem;
      label {
        display: block;
        font-size: 0.85rem;
        font-weight: 600;
        color: #374151;
        margin-bottom: 0.35rem;
      }
      input {
        width: 100%;
        box-sizing: border-box;
        padding: 0.75rem 1rem;
        border: 1.5px solid #d1d5db;
        border-radius: 8px;
        font-size: 0.95rem;
        &:focus {
          outline: none;
          border-color: #10b981;
          box-shadow: 0 0 0 3px rgba(16, 185, 129, 0.15);
        }
      }
    }
    .address-type-selector {
      margin-top: 0.5rem;
      label {
        display: block;
        font-size: 0.85rem;
        font-weight: 600;
        color: #374151;
        margin-bottom: 0.5rem;
      }
      .type-pills {
        display: flex;
        gap: 0.75rem;
        button {
          background: #f3f4f6;
          border: 1px solid #e5e7eb;
          padding: 0.45rem 1rem;
          border-radius: 9999px;
          font-size: 0.85rem;
          font-weight: 600;
          cursor: pointer;
          color: #4b5563;
          &.active {
            background: #ecfdf5;
            border-color: #10b981;
            color: #065f46;
          }
        }
      }
    }
    .payment-options {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
    }
    .payment-option {
      display: flex;
      align-items: center;
      gap: 1rem;
      padding: 1rem;
      border: 1.5px solid #e5e7eb;
      border-radius: 12px;
      cursor: pointer;
      transition: all 0.15s ease;
      &.selected {
        border-color: #10b981;
        background: #f0fdf4;
      }
      input[type="radio"] {
        accent-color: #10b981;
        transform: scale(1.2);
      }
      .option-content {
        display: flex;
        align-items: center;
        gap: 0.85rem;
        .option-icon { font-size: 1.5rem; }
        strong { display: block; font-size: 0.95rem; color: #1f2937; }
        p { margin: 0.15rem 0 0; font-size: 0.8rem; color: #6b7280; }
      }
    }
    .summary-card {
      background: #ffffff;
      border: 1px solid #e5e7eb;
      border-radius: 16px;
      padding: 1.5rem;
      box-shadow: 0 1px 3px rgba(0,0,0,0.05);
      position: sticky;
      top: 90px;
      h3 {
        margin: 0 0 1rem 0;
        font-size: 1.15rem;
        color: #1f2937;
        padding-bottom: 0.75rem;
        border-bottom: 1px solid #f3f4f6;
      }
    }
    .items-preview {
      max-height: 200px;
      overflow-y: auto;
      margin-bottom: 1rem;
      .summary-item {
        display: flex;
        justify-content: space-between;
        margin-bottom: 0.5rem;
        font-size: 0.88rem;
        .name { color: #374151; font-weight: 500; }
        .qty { color: #9ca3af; margin-left: 0.4rem; font-size: 0.8rem; }
        .price { font-weight: 600; color: #111827; }
      }
    }
    .bill-breakdown {
      border-top: 1px dashed #e5e7eb;
      padding-top: 1rem;
      margin-bottom: 1.25rem;
      .bill-row {
        display: flex;
        justify-content: space-between;
        margin-bottom: 0.5rem;
        font-size: 0.9rem;
        color: #4b5563;
        .free-badge { color: #10b981; font-weight: 700; }
        &.grand-total {
          border-top: 1px solid #e5e7eb;
          padding-top: 0.75rem;
          margin-top: 0.75rem;
          font-weight: 700;
          font-size: 1.1rem;
          color: #111827;
          .grand-price { color: #10b981; }
        }
      }
    }
    .btn-place-order {
      width: 100%;
      background: #10b981;
      color: #fff;
      border: none;
      padding: 1rem;
      border-radius: 12px;
      font-weight: 700;
      font-size: 1.05rem;
      cursor: pointer;
      box-shadow: 0 4px 14px rgba(16, 185, 129, 0.35);
      transition: background 0.15s;
      &:hover:not(:disabled) {
        background: #059669;
      }
      &:disabled {
        opacity: 0.6;
        cursor: not-allowed;
      }
    }
    .trust-notice {
      text-align: center;
      margin-top: 0.75rem;
      font-size: 0.8rem;
      color: #6b7280;
    }
  `]
})
export class CheckoutComponent implements OnInit {
  readonly cart = inject(CartService);
  private readonly orderService = inject(OrderService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly isSubmitting = signal(false);
  selectedPayment: PaymentMethod = 'COD';

  address: ShippingAddress = {
    fullName: '',
    phoneNumber: '',
    streetAddress: '',
    city: 'Bengaluru',
    state: 'Karnataka',
    postalCode: '560001',
    addressType: 'Home'
  };

  ngOnInit(): void {
    const user = this.authService.currentUser();
    if (user) {
      this.address.fullName = user.fullName || '';
      this.address.phoneNumber = user.phoneNumber || '';
    }
  }

  placeOrder(): void {
    if (!this.address.fullName || !this.address.phoneNumber || !this.address.streetAddress) {
      alert('Please fill in your recipient name, phone number, and delivery street address.');
      return;
    }

    this.isSubmitting.set(true);

    const itemsDto = this.cart.items().map(item => ({
      productId: item.productId,
      productName: item.name,
      unitPrice: item.price,
      quantity: item.quantity,
      totalPrice: item.price * item.quantity,
      imageUrl: item.imageUrl,
      unitOfMeasure: item.unitOfMeasure
    }));

    const orderRequest: CreateOrderRequest = {
      shippingAddress: this.address,
      paymentMethod: this.selectedPayment,
      items: itemsDto,
      subtotal: this.cart.subtotal(),
      deliveryFee: this.cart.deliveryFee(),
      totalAmount: this.cart.totalAmount() + 2 // including ₹2 handling fee
    };

    this.orderService.createOrder(orderRequest).subscribe({
      next: (res) => {
        this.cart.clearCart();
        this.isSubmitting.set(false);
        this.router.navigate(['/order-success', res.id]);
      },
      error: () => {
        this.isSubmitting.set(false);
        alert('Order processing encountered an issue. Please try again.');
      }
    });
  }
}

