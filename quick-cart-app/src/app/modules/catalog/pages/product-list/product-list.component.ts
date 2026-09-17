import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CatalogService } from '../../services/catalog.service';
import { Product, Category } from '../../models/product.model';
import { ProductCardComponent } from '../../components/product-card/product-card.component';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, ProductCardComponent],
  template: `
    <div class="page-container">
      <!-- Promo Hero Banner -->
      <div class="promo-banner">
        <div class="promo-content">
          <span class="promo-badge">🎉 Special Launch Offer</span>
          <h2>Grocery Delivery in <span class="highlight">10 Minutes</span></h2>
          <p>Order fresh daily essentials, snacks, and dairy delivered right to your doorstep.</p>
          <div class="coupon-tag">Use code <strong>QUICK20</strong> for 20% OFF</div>
        </div>
        <div class="promo-icon">⚡🛍️</div>
      </div>

      <!-- Category Filter Chips -->
      <div class="category-scroll-bar">
        @for (cat of categories(); track cat.id) {
          <button
            class="cat-chip"
            [class.active]="activeCategory() === cat.slug"
            (click)="selectCategory(cat.slug)"
          >
            <span class="cat-icon">{{ cat.iconUrl }}</span>
            <span class="cat-name">{{ cat.name }}</span>
          </button>
        }
      </div>

      <!-- Section Title & Count -->
      <div class="section-header">
        <h2>
          @if (activeCategory() === 'all') {
            Trending Essentials
          } @else {
            Category Items
          }
        </h2>
        <span class="item-count">{{ products().length }} products</span>
      </div>

      <!-- Products Grid -->
      @if (isLoading()) {
        <div class="loading-state">
          <div class="spinner"></div>
          <span>Loading freshest items for you...</span>
        </div>
      } @else if (products().length === 0) {
        <div class="empty-state">
          <span>🔍</span>
          <h3>No products found</h3>
          <p>Try switching categories or searching for something else.</p>
        </div>
      } @else {
        <div class="product-grid">
          @for (prod of products(); track prod.id) {
            <app-product-card [product]="prod" />
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .page-container {
      display: flex;
      flex-direction: column;
      gap: 1.75rem;
    }
    .promo-banner {
      background: linear-gradient(135deg, #064e3b 0%, #047857 60%, #10b981 100%);
      color: #ffffff;
      border-radius: 18px;
      padding: 2.25rem 2.5rem;
      display: flex;
      justify-content: space-between;
      align-items: center;
      box-shadow: 0 10px 25px rgba(4, 120, 87, 0.2);
    }
    .promo-badge {
      background: rgba(255, 255, 255, 0.2);
      font-size: 0.8rem;
      font-weight: 700;
      padding: 0.25rem 0.65rem;
      border-radius: 6px;
      display: inline-block;
      margin-bottom: 0.6rem;
    }
    .promo-content h2 {
      font-size: 2rem;
      font-weight: 800;
      margin: 0 0 0.5rem;
      letter-spacing: -0.5px;
    }
    .highlight {
      color: #fef08a;
    }
    .promo-content p {
      font-size: 1rem;
      margin: 0 0 1rem;
      opacity: 0.9;
    }
    .coupon-tag {
      background: #ffffff;
      color: #065f46;
      font-size: 0.85rem;
      font-weight: 600;
      padding: 0.35rem 0.8rem;
      border-radius: 8px;
      display: inline-block;
    }
    .promo-icon {
      font-size: 4.5rem;
      opacity: 0.9;
    }
    .category-scroll-bar {
      display: flex;
      gap: 0.75rem;
      overflow-x: auto;
      padding: 0.5rem 0.25rem;
      scrollbar-width: thin;
    }
    .cat-chip {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      background: #ffffff;
      border: 1px solid #e5e7eb;
      padding: 0.6rem 1.1rem;
      border-radius: 9999px;
      font-size: 0.9rem;
      font-weight: 600;
      color: #374151;
      cursor: pointer;
      white-space: nowrap;
      transition: all 0.15s ease;
    }
    .cat-chip:hover {
      border-color: #10b981;
      background: #f0fdf4;
      color: #059669;
    }
    .cat-chip.active {
      background: #10b981;
      border-color: #10b981;
      color: #ffffff;
      box-shadow: 0 4px 12px rgba(16, 185, 129, 0.25);
    }
    .section-header {
      display: flex;
      justify-content: space-between;
      align-items: baseline;
      border-bottom: 2px solid #e5e7eb;
      padding-bottom: 0.75rem;
    }
    .section-header h2 {
      font-size: 1.4rem;
      font-weight: 800;
      color: #111827;
      margin: 0;
    }
    .item-count {
      font-size: 0.9rem;
      color: #6b7280;
      font-weight: 600;
    }
    .product-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
      gap: 1.25rem;
    }
    .loading-state, .empty-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 4rem 2rem;
      text-align: center;
      color: #6b7280;
    }
    .spinner {
      width: 36px;
      height: 36px;
      border: 3px solid #e5e7eb;
      border-top-color: #10b981;
      border-radius: 50%;
      animation: spin 0.7s linear infinite;
      margin-bottom: 1rem;
    }
    @keyframes spin {
      to { transform: rotate(360deg); }
    }
  `]
})
export class ProductListComponent implements OnInit {
  private readonly catalogService = inject(CatalogService);

  readonly categories = signal<Category[]>([]);
  readonly products = signal<Product[]>([]);
  readonly activeCategory = signal<string>('all');
  readonly isLoading = signal<boolean>(true);

  ngOnInit(): void {
    this.loadCategories();
    this.loadProducts();
  }

  loadCategories(): void {
    this.catalogService.getCategories().subscribe((cats) => {
      this.categories.set(cats);
    });
  }

  loadProducts(): void {
    this.isLoading.set(true);
    this.catalogService.getProducts(this.activeCategory()).subscribe({
      next: (prods) => {
        this.products.set(prods);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  selectCategory(slug: string): void {
    this.activeCategory.set(slug);
    this.loadProducts();
  }
}
