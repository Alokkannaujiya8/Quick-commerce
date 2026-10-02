# QuickCart Frontend — State Management Architecture

This document specifies the actual state management patterns implemented in the QuickCart frontend. The application utilizes a hybrid reactivity model combining **Angular Signals** for local and cross-component synchronous state with **RxJS Observables** for asynchronous operations and HTTP streams.

---

## 1. State Management Philosophy

* **No Heavy Third-Party Libraries**: The application does not use NgRx, Akita, or NGXS.
* **Angular 20 Native Reactivity**: State is owned by injectable root services (`@Injectable({ providedIn: 'root' })`) using Angular Signals (`signal`, `computed`, `effect`).
* **Unidirectional Data Flow**: Components read signals, invoke methods on services to mutate state, and render views based on computed signals.

---

## 2. Shared State Stores (Root Services)

### 2.1 Cart State (`CartService`)
Manages the customer's active shopping cart, line-item quantities, subtotal calculations, and drawer visibility:
* **Signals**:
  * `items = signal<CartItem[]>(this.loadStoredItems())`: List of active items in the cart.
  * `isDrawerOpen = signal<boolean>(false)`: Slide-out cart drawer toggle state.
* **Computed Values**:
  * `totalItems = computed(() => this.items().reduce((t, i) => t + i.quantity, 0))`: Total count of items.
  * `subtotal = computed(() => this.items().reduce((t, i) => t + i.lineTotal, 0))`: Aggregate price before fees.
  * `deliveryFee = computed(() => sub === 0 ? 0 : sub >= 500 ? 0 : 40)`: Free delivery over ₹500, else ₹40.
  * `totalAmount = computed(() => this.subtotal() + this.deliveryFee())`: Final checkout amount.
* **Persistence**: Persisted to `localStorage` under `qc_cart_items`.

### 2.2 Authentication State (`AuthService`)
Manages identity credentials, authentication status, and active session tokens:
* **Signals**:
  * `currentUser = signal<User | null>(this.getStoredUser())`: Profile of currently authenticated user.
  * `user = this.currentUser.asReadonly()`: Public read-only projection.
  * `isAuthenticated = computed(() => !!this.currentUser() || !!this.getAccessToken())`: Reactive auth boolean.
  * `pendingPhoneNumber = signal<string>('')`: Retains phone number during the multi-step OTP login flow.
* **Persistence**: Synchronized with `localStorage` (`qc_access_token`, `qc_refresh_token`, `qc_user_profile`).

### 2.3 Theme State (`ThemeService`)
Manages user color scheme preferences and DOM class bindings:
* **Signals**:
  * `theme = signal<AppTheme>(this.getInitialTheme())`: Current theme (`'light'` | `'dark'`).
  * `isDark = computed(() => this.theme() === 'dark')`: Boolean helper for template switches.
* **Effects**:
  * `effect(() => { this.applyThemeToDom(this.theme()); })`: Automatically synchronizes `data-theme` and `dark-theme` classes on `<html>` and `<body>` tags whenever `theme` changes.
* **Persistence**: Synchronized with `localStorage` (`qc_theme`).

### 2.4 Order & Payment State (`OrderService`, `PaymentService`)
* `OrderService`:
  * `currentOrder = signal<OrderResponse | null>(null)`
  * `orderHistory = signal<OrderResponse[]>(this.loadOrdersFromStorage())`
  * Persisted to `localStorage` under `quickcart_orders`.
* `PaymentService`:
  * `activeIntent = signal<PaymentIntentResponse | null>(null)`
  * `latestPayment = signal<PaymentDto | null>(null)`

---

## 3. Local Component State

Components maintain localized UI state using lightweight signals:
* **Loading State**: `isLoading = signal(false)`, `isGoogleLoading = signal(false)`, `isSubmitting = signal(false)`.
* **Form Feedback**: `errorMessage = signal<string | null>(null)`, `toastMessage = signal<string | null>(null)`.
* **View Filters**: `activeCategory = signal<string>('all')`.

---

## 4. Asynchronous State with RxJS

* HTTP requests are dispatched through `ApiService` returning `Observable<T>`.
* Asynchronous state pipelines are orchestrated using operators like `pipe()`, `tap()`, `switchMap()`, and `catchError()`.
* Signals are updated inside `.pipe(tap(...))` or `.subscribe({ next: ... })` blocks to trigger immediate change detection.
