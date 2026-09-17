import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { OrderService } from '../../services/order.service';
import { OrderResponse } from '../../models/order.model';

@Component({
  selector: 'app-order-success',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="success-page">
      <div class="success-card">
        <!-- Confirmed Banner -->
        <div class="header-section">
          <div class="success-icon">✓</div>
          <h2>Order Confirmed!</h2>
          <p class="subtitle">Sit back and relax! Your groceries are arriving in ~10 minutes.</p>
        </div>

        @if (order(); as ord) {
          <!-- Quick Info Bar -->
          <div class="order-quick-info">
            <div>
              <span class="label">Order ID</span>
              <strong class="value">{{ ord.orderNumber }}</strong>
            </div>
            <div>
              <span class="label">Payment</span>
              <strong class="value">{{ ord.paymentMethod }} ({{ ord.paymentStatus }})</strong>
            </div>
            <div>
              <span class="label">Total Paid</span>
              <strong class="value highlight">₹{{ ord.totalAmount | number:'1.2-2' }}</strong>
            </div>
          </div>

          <!-- Live 10-Min Delivery Timeline -->
          <div class="tracker-section">
            <h4>Live Order Status</h4>
            <div class="tracker-steps">
              <div class="step completed">
                <div class="step-dot">✓</div>
                <div class="step-text">
                  <strong>Order Placed</strong>
                  <span>Confirmed with store</span>
                </div>
              </div>
              <div class="step-line active"></div>
              <div class="step active">
                <div class="step-dot pulse">🛒</div>
                <div class="step-text">
                  <strong>Packing Items</strong>
                  <span>Dark store agent picking fresh</span>
                </div>
              </div>
              <div class="step-line"></div>
              <div class="step">
                <div class="step-dot">🛵</div>
                <div class="step-text">
                  <strong>Out for Delivery</strong>
                  <span>Rider on the way</span>
                </div>
              </div>
              <div class="step-line"></div>
              <div class="step">
                <div class="step-dot">🏠</div>
                <div class="step-text">
                  <strong>Delivered</strong>
                  <span>At your doorstep</span>
                </div>
              </div>
            </div>
          </div>

          <!-- Delivery Address & Items -->
          <div class="details-grid">
            <div class="address-box">
              <h4>Delivering To</h4>
              <p class="name"><strong>{{ ord.shippingAddress.fullName }}</strong> ({{ ord.shippingAddress.addressType }})</p>
              <p class="phone">📞 {{ ord.shippingAddress.phoneNumber }}</p>
              <p class="street">{{ ord.shippingAddress.streetAddress }}, {{ ord.shippingAddress.city }} - {{ ord.shippingAddress.postalCode }}</p>
            </div>

            <div class="items-box">
              <h4>Items in this order ({{ ord.items.length }})</h4>
              <div class="order-items-list">
                @for (item of ord.items; track item.productId) {
                  <div class="item-row">
                    <span class="item-qty-name">{{ item.quantity }}x {{ item.productName }}</span>
                    <span class="item-price">₹{{ item.totalPrice | number:'1.2-2' }}</span>
                  </div>
                }
              </div>
            </div>
          </div>
        } @else {
          <div class="loading-box">
            <p>Loading your order details...</p>
          </div>
        }

        <div class="action-footer">
          <a routerLink="/" class="btn-home">Continue Shopping</a>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .success-page {
      padding: 2rem 1rem 4rem;
      display: flex;
      justify-content: center;
    }
    .success-card {
      max-width: 720px;
      width: 100%;
      background: #ffffff;
      border: 1px solid #e5e7eb;
      border-radius: 20px;
      padding: 2.5rem 2rem;
      box-shadow: 0 10px 25px rgba(0, 0, 0, 0.04);
    }
    .header-section {
      text-align: center;
      margin-bottom: 2rem;
      .success-icon {
        width: 68px;
        height: 68px;
        background: #10b981;
        color: white;
        font-size: 2.2rem;
        font-weight: 800;
        border-radius: 50%;
        display: flex;
        align-items: center;
        justify-content: center;
        margin: 0 auto 1.25rem;
        box-shadow: 0 8px 20px rgba(16, 185, 129, 0.3);
      }
      h2 {
        font-size: 1.8rem;
        font-weight: 800;
        color: #111827;
        margin: 0 0 0.5rem;
      }
      .subtitle {
        color: #4b5563;
        font-size: 1rem;
        margin: 0;
      }
    }
    .order-quick-info {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      background: #f9fafb;
      border: 1px solid #f3f4f6;
      border-radius: 12px;
      padding: 1rem;
      text-align: center;
      gap: 0.5rem;
      margin-bottom: 2rem;
      .label {
        display: block;
        font-size: 0.75rem;
        color: #6b7280;
        text-transform: uppercase;
        font-weight: 600;
        margin-bottom: 0.25rem;
      }
      .value {
        font-size: 0.95rem;
        color: #1f2937;
        &.highlight { color: #10b981; font-size: 1.1rem; }
      }
      @media (max-width: 600px) {
        grid-template-columns: 1fr;
        text-align: left;
      }
    }
    .tracker-section {
      background: #fdfdfd;
      border: 1px solid #e5e7eb;
      border-radius: 14px;
      padding: 1.5rem;
      margin-bottom: 2rem;
      h4 {
        margin: 0 0 1.25rem;
        font-size: 1.05rem;
        color: #111827;
      }
      .tracker-steps {
        display: flex;
        align-items: center;
        justify-content: space-between;
        @media (max-width: 640px) {
          flex-direction: column;
          align-items: flex-start;
          gap: 1rem;
        }
      }
      .step {
        display: flex;
        flex-direction: column;
        align-items: center;
        text-align: center;
        gap: 0.4rem;
        @media (max-width: 640px) {
          flex-direction: row;
          text-align: left;
          gap: 0.75rem;
        }
        .step-dot {
          width: 38px;
          height: 38px;
          border-radius: 50%;
          background: #e5e7eb;
          color: #9ca3af;
          display: flex;
          align-items: center;
          justify-content: center;
          font-weight: 700;
          font-size: 1rem;
        }
        &.completed .step-dot {
          background: #10b981;
          color: white;
        }
        &.active .step-dot {
          background: #3b82f6;
          color: white;
          box-shadow: 0 0 0 4px rgba(59, 130, 246, 0.2);
        }
        .step-text {
          strong {
            display: block;
            font-size: 0.85rem;
            color: #1f2937;
          }
          span {
            font-size: 0.72rem;
            color: #6b7280;
          }
        }
      }
      .step-line {
        flex: 1;
        height: 3px;
        background: #e5e7eb;
        margin: 0 0.5rem;
        margin-bottom: 1.8rem;
        &.active {
          background: #10b981;
        }
        @media (max-width: 640px) {
          display: none;
        }
      }
    }
    .details-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1.5rem;
      margin-bottom: 2rem;
      @media (max-width: 640px) {
        grid-template-columns: 1fr;
      }
      .address-box, .items-box {
        background: #f9fafb;
        border-radius: 12px;
        padding: 1.25rem;
        h4 {
          margin: 0 0 0.75rem;
          font-size: 0.95rem;
          color: #374151;
          border-bottom: 1px solid #e5e7eb;
          padding-bottom: 0.5rem;
        }
        p {
          margin: 0.25rem 0;
          font-size: 0.85rem;
          color: #4b5563;
        }
      }
      .order-items-list {
        max-height: 150px;
        overflow-y: auto;
        .item-row {
          display: flex;
          justify-content: space-between;
          font-size: 0.85rem;
          padding: 0.35rem 0;
          border-bottom: 1px dashed #e5e7eb;
          .item-price { font-weight: 600; color: #111827; }
        }
      }
    }
    .action-footer {
      text-align: center;
      .btn-home {
        display: inline-block;
        background: #10b981;
        color: white;
        padding: 0.85rem 2.5rem;
        border-radius: 12px;
        font-weight: 700;
        font-size: 1rem;
        text-decoration: none;
        box-shadow: 0 4px 14px rgba(16, 185, 129, 0.3);
        &:hover { background: #059669; }
      }
    }
    .loading-box {
      text-align: center;
      padding: 2rem;
      color: #6b7280;
    }
  `]
})
export class OrderSuccessComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly orderService = inject(OrderService);

  readonly order = signal<OrderResponse | null>(null);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.orderService.getOrderById(id).subscribe(res => {
        if (res) {
          this.order.set(res);
        } else if (this.orderService.currentOrder()) {
          this.order.set(this.orderService.currentOrder());
        }
      });
    } else {
      this.order.set(this.orderService.currentOrder());
    }
  }
}
