# State Management Architectural Specification

This document details the reactive state management architecture of the QuickCart frontend, specifying how Angular Signals, RxJS streams, service stores, and browser local storage interact to provide a high-performance, deterministic state model.

---

## 1. Architectural Model Overview

```text
┌────────────────────────────────────────────────────────┐
│                   Angular Signals                      │
│  (Synchronous, In-Memory, Fine-Grained, Deterministic) │
│                                                        │
│  • signal()    -> Writable domain & UI primitives      │
│  • computed()  -> Pure derived projections             │
│  • effect()    -> Targeted side-effects (DOM, Storage) │
└───────────────────────────▲────────────────────────────┘
                            │
               Sync / Update│ (tap, subscribe)
                            │
┌───────────────────────────┴────────────────────────────┐
│                    RxJS Observables                    │
│   (Asynchronous, Network Streams, Operators, Events)   │
│                                                        │
│  • HttpClient  -> GET, POST, PUT, DELETE               │
│  • Operators   -> switchMap, map, catchError           │
│  • Events      -> GIS Script loading, retries          │
└───────────────────────────▲────────────────────────────┘
                            │
                     Network│ REST Calls
                            │
┌───────────────────────────┴────────────────────────────┐
│               QuickCart Backend API Host               │
└────────────────────────────────────────────────────────┘
```

---

## 2. Shared Domain State Stores

### 2.1 Cart State Store (`CartService`)
The cart state store manages items in the current grocery cart, auto-calculates taxes and delivery thresholds, and controls the cart slide-out drawer:

```typescript
@Injectable({ providedIn: 'root' })
export class CartService {
  // Mutable State
  readonly items = signal<CartItem[]>(this.loadStoredItems());
  readonly isDrawerOpen = signal<boolean>(false);

  // Derived State (Computed & Memoized)
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
}
```

* **Local Storage Integration**: Every mutation (`addToCart`, `updateQuantity`, `removeItem`, `clearCart`) calls `updateState()`, which pushes the new array to `items.set(items)` and writes JSON to `localStorage.setItem('qc_cart_items', ...)`.

### 2.2 Authentication State Store (`AuthService`)
* **Mutable State**:
  * `currentUser = signal<User | null>(this.getStoredUser())`
  * `pendingPhoneNumber = signal<string>('')`
* **Exposed Read-Only**:
  * `user = this.currentUser.asReadonly()`
* **Derived State**:
  * `isAuthenticated = computed(() => !!this.currentUser() || !!this.getAccessToken())`
* **Local Storage Integration**: Tokens and profile are synchronized under `qc_access_token`, `qc_refresh_token`, and `qc_user_profile`.

### 2.3 Theme State Store (`ThemeService`)
* **Mutable State**:
  * `theme = signal<AppTheme>(this.getInitialTheme())`
* **Derived State**:
  * `isDark = computed(() => this.theme() === 'dark')`
* **Effect Hook**:
  * `effect(() => this.applyThemeToDom(this.theme()))` ensures DOM attributes (`data-theme`, `dark-theme`) reflect the current signal state immediately and persist to `localStorage.setItem('qc_theme', ...)`.

---

## 3. Local Component State

Components declare signals for page-level concerns:
* `isSubmitting = signal(false)`: Prevents duplicate submissions in `CheckoutComponent`.
* `activeCategory = signal('all')`: Tracks the active category chip filter in `ProductListComponent`.
* `toastMessage = signal<string | null>(null)`: Controls error/notification toasts in `LoginComponent`.

---

## 4. Why Native Signals Instead of NgRx?

1. **Zero Boilerplate**: No actions, reducers, effects classes, or selector factories required for simple quick-commerce workflows.
2. **Deep Zone.js & Signal Interoperability**: Signals trigger OnPush component updates directly, eliminating zone pollution.
3. **Glitch-Free Computed Derivations**: `totalItems` and `totalAmount` update synchronously without race conditions.
4. **Minimal Bundle Footprint**: Zero external package overhead.
