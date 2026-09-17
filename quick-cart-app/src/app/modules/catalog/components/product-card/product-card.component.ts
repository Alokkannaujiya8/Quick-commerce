import { Component, Input, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Product } from '../../models/product.model';
import { CartService } from '../../../cart/services/cart.service';

@Component({
  selector: 'app-product-card',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="card">
      <div class="image-wrapper">
        @if (product.imageUrl) {
          <img [src]="product.imageUrl" [alt]="product.name" class="product-img" loading="lazy" />
        } @else {
          <div class="img-placeholder">🛍️</div>
        }
        <span class="delivery-tag">⚡ 10 MINS</span>
      </div>

      <div class="card-content">
        <h3 class="product-title" [title]="product.name">{{ product.name }}</h3>
        <span class="product-unit">{{ product.unitOfMeasure }}</span>

        <div class="price-action-row">
          <div class="price-container">
            <span class="price-current">₹{{ product.price }}</span>
            @if (product.originalPrice && product.originalPrice > product.price) {
              <span class="price-original">₹{{ product.originalPrice }}</span>
            }
          </div>

          <!-- Add / Quantity Controls -->
          @if (quantityInCart() > 0) {
            <div class="qty-control">
              <button (click)="cart.updateQuantity(product.id, -1)" class="btn-qty">-</button>
              <span class="qty-num">{{ quantityInCart() }}</span>
              <button (click)="cart.updateQuantity(product.id, 1)" class="btn-qty">+</button>
            </div>
          } @else {
            <button (click)="onAddToCart()" class="btn-add">
              ADD
            </button>
          }
        </div>
      </div>
    </div>
  `,
  styles: [`
    .card {
      background: #ffffff;
      border: 1px solid #e5e7eb;
      border-radius: 14px;
      overflow: hidden;
      display: flex;
      flex-direction: column;
      transition: all 0.2s ease;
      position: relative;
    }
    .card:hover {
      box-shadow: 0 8px 20px rgba(0, 0, 0, 0.06);
      border-color: #d1d5db;
      transform: translateY(-2px);
    }
    .image-wrapper {
      position: relative;
      height: 160px;
      background: #f9fafb;
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 0.75rem;
    }
    .product-img {
      max-height: 100%;
      max-width: 100%;
      object-fit: contain;
    }
    .img-placeholder {
      font-size: 3rem;
    }
    .delivery-tag {
      position: absolute;
      top: 8px;
      left: 8px;
      background: rgba(255, 255, 255, 0.92);
      backdrop-filter: blur(4px);
      font-size: 0.68rem;
      font-weight: 800;
      color: #047857;
      padding: 0.2rem 0.45rem;
      border-radius: 6px;
      box-shadow: 0 1px 4px rgba(0, 0, 0, 0.08);
    }
    .card-content {
      padding: 1rem;
      display: flex;
      flex-direction: column;
      flex: 1;
    }
    .product-title {
      font-size: 0.95rem;
      font-weight: 600;
      color: #1f2937;
      margin: 0 0 0.3rem;
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
      overflow: hidden;
      min-height: 2.5rem;
      line-height: 1.25;
    }
    .product-unit {
      font-size: 0.78rem;
      color: #6b7280;
      margin-bottom: 0.75rem;
    }
    .price-action-row {
      margin-top: auto;
      display: flex;
      align-items: center;
      justify-content: space-between;
    }
    .price-container {
      display: flex;
      align-items: baseline;
      gap: 0.35rem;
    }
    .price-current {
      font-size: 1.05rem;
      font-weight: 800;
      color: #111827;
    }
    .price-original {
      font-size: 0.8rem;
      color: #9ca3af;
      text-decoration: line-through;
    }
    .btn-add {
      background: #f0fdf4;
      border: 1px solid #10b981;
      color: #059669;
      font-size: 0.85rem;
      font-weight: 800;
      padding: 0.35rem 1rem;
      border-radius: 8px;
      cursor: pointer;
      transition: all 0.15s ease;
    }
    .btn-add:hover {
      background: #10b981;
      color: #ffffff;
    }
    .qty-control {
      display: flex;
      align-items: center;
      background: #10b981;
      border-radius: 8px;
      overflow: hidden;
    }
    .btn-qty {
      background: none;
      border: none;
      color: #ffffff;
      padding: 0.35rem 0.65rem;
      font-weight: 800;
      cursor: pointer;
      font-size: 0.9rem;
    }
    .qty-num {
      color: #ffffff;
      font-weight: 800;
      font-size: 0.85rem;
      min-width: 18px;
      text-align: center;
    }
  `]
})
export class ProductCardComponent {
  @Input({ required: true }) product!: Product;
  readonly cart = inject(CartService);

  readonly quantityInCart = computed(() => {
    const item = this.cart.items().find((i) => i.productId === this.product.id);
    return item ? item.quantity : 0;
  });

  onAddToCart(): void {
    this.cart.addToCart({
      id: this.product.id,
      name: this.product.name,
      sku: this.product.sku,
      unitOfMeasure: this.product.unitOfMeasure,
      price: this.product.price,
      imageUrl: this.product.imageUrl
    });
  }
}
