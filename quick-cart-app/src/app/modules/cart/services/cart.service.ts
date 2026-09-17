import { Injectable, signal, computed } from '@angular/core';
import { CartItem } from '../models/cart.model';

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private readonly storageKey = 'qc_cart_items';

  // Signals for Reactive State
  readonly items = signal<CartItem[]>(this.loadStoredItems());
  readonly isDrawerOpen = signal<boolean>(false);

  readonly totalItems = computed(() =>
    this.items().reduce((total, item) => total + item.quantity, 0)
  );

  readonly subtotal = computed(() =>
    this.items().reduce((total, item) => total + item.lineTotal, 0)
  );

  readonly deliveryFee = computed(() => {
    const sub = this.subtotal();
    return sub === 0 ? 0 : sub >= 500 ? 0 : 40;
  });

  readonly totalAmount = computed(() => this.subtotal() + this.deliveryFee());

  addToCart(product: {
    id: string;
    name: string;
    sku: string;
    unitOfMeasure: string;
    price: number;
    imageUrl?: string;
  }, quantity: number = 1): void {
    const current = [...this.items()];
    const index = current.findIndex((i) => i.productId === product.id);

    if (index > -1) {
      const existing = current[index];
      const newQty = existing.quantity + quantity;
      current[index] = {
        ...existing,
        quantity: newQty,
        lineTotal: newQty * existing.price
      };
    } else {
      current.push({
        id: crypto.randomUUID(),
        productId: product.id,
        name: product.name,
        sku: product.sku,
        unitOfMeasure: product.unitOfMeasure,
        price: product.price,
        imageUrl: product.imageUrl,
        quantity: quantity,
        lineTotal: quantity * product.price
      });
    }

    this.updateState(current);
    this.isDrawerOpen.set(true); // auto-open drawer on add
  }

  updateQuantity(productId: string, delta: number): void {
    const current = this.items()
      .map((item) => {
        if (item.productId === productId) {
          const newQty = item.quantity + delta;
          return newQty > 0
            ? { ...item, quantity: newQty, lineTotal: newQty * item.price }
            : null;
        }
        return item;
      })
      .filter((item): item is CartItem => item !== null);

    this.updateState(current);
  }

  removeItem(productId: string): void {
    const current = this.items().filter((i) => i.productId !== productId);
    this.updateState(current);
  }

  clearCart(): void {
    this.updateState([]);
  }

  toggleDrawer(): void {
    this.isDrawerOpen.update((open) => !open);
  }

  openDrawer(): void {
    this.isDrawerOpen.set(true);
  }

  closeDrawer(): void {
    this.isDrawerOpen.set(false);
  }

  private updateState(items: CartItem[]): void {
    this.items.set(items);
    localStorage.setItem(this.storageKey, JSON.stringify(items));
  }

  private loadStoredItems(): CartItem[] {
    try {
      const data = localStorage.getItem(this.storageKey);
      return data ? JSON.parse(data) : [];
    } catch {
      return [];
    }
  }
}
