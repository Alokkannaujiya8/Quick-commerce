# Components Architecture Specification

This document details the component architecture, standalone declarations, change detection strategies, and template control flow conventions used across the QuickCart frontend.

---

## 1. Component Design Paradigm

* **100% Standalone Components**: Every component declares `standalone: true` and defines explicit dependencies in its `imports: [...]` array.
* **Modern Control Flow**: Templates strictly employ Angular built-in control flow:
  * Conditional Rendering: `@if`, `@else if`, `@else`.
  * Collection Rendering: `@for (item of items; track item.id)`.
  * Empty Collection Fallback: `@empty`.
* **Change Detection Strategy**: All interactive components implement `changeDetection: ChangeDetectionStrategy.OnPush` to minimize change detection cycles.

---

## 2. Component Inventory

### 2.1 Layout Components (`src/app/layout/`)

| Component | Selector | Responsibilities & Injected Services |
| :--- | :--- | :--- |
| `MainLayoutComponent` | `app-main-layout` | Composes `HeaderComponent`, `<router-outlet />`, `CartDrawerComponent`, and `FooterComponent`. Provides fixed layout constraints (`max-width: 1280px`). |
| `HeaderComponent` | `app-header` | Injects `CartService`, `AuthService`. Displays delivery estimate (`10-15 MINS`), search bar, theme toggle, user authentication state (`@if (auth.currentUser(); as user)`), and live cart counter badge. |
| `FooterComponent` | `app-footer` | Informational footer with store links, service delivery promises, and copyright information. |

### 2.2 Feature Page Components (`src/app/modules/`)

| Component | Selector | Responsibilities & Injected Services |
| :--- | :--- | :--- |
| `ProductListComponent` | `app-product-list` | Injects `CatalogService`. Manages `categories`, `products`, `activeCategory`, and `isLoading` signals. Renders promo hero banner, category chips, and product grid. |
| `CheckoutComponent` | `app-checkout` | Injects `CartService`, `OrderService`, `PaymentService`, `AuthService`, `Router`. Manages delivery address input, payment method selection, order placement, and payment verification flow. |
| `OrderSuccessComponent` | `app-order-success` | Injects `ActivatedRoute`, `OrderService`, `PaymentService`. Displays order confirmation number, delivery timeline, payment status badge, and itemized receipt. |
| `LoginComponent` | `app-login` | Injects `AuthService`, `GoogleIdentityService`, `Router`. Supports 10-digit phone number entry for OTP dispatch and Google One-Click sign-in. |
| `OtpVerifyComponent` | `app-otp-verify` | Injects `AuthService`, `ActivatedRoute`, `Router`. Captures 6-digit verification code, handles resend action, and executes verification. |
| `PasswordLoginComponent` | `app-password-login`| Injects `AuthService`, `Router`. Provides email/password login credentials entry and ProblemDetails error display. |
| `RegisterComponent` | `app-register` | Injects `AuthService`, `Router`. Customer account creation with full name, phone number, email, and password confirmation. |

### 2.3 Shared Presenter Components (`src/app/shared/components/`)

| Component | Selector | Inputs / Outputs | Purpose |
| :--- | :--- | :--- | :--- |
| `UiButtonComponent` | `app-ui-button` | Inputs: `type`, `variant`, `size`, `disabled`, `loading`, `fullWidth`. Output: `clicked`. | Primary UI action button supporting loading spinner and custom states. |
| `AlertBannerComponent` | `app-alert-banner` | Inputs: `variant` (`'danger'` \| `'success'`), `dismissible`. Output: `close`. | Accessible feedback banner for errors and notifications. |
| `AuthCardShellComponent` | `app-auth-card-shell` | Content projection (`<ng-content>`). | Standard card shell and background gradient for auth pages. |
| `BrandLogoComponent` | `app-brand-logo` | Inputs: `size` (`'sm'` \| `'md'` \| `'lg'`), `variant` (`'full'` \| `'inline'`). | SVG logo and "QuickCart" branding. |
| `ThemeToggleComponent` | `app-theme-toggle` | Inputs: `showLabel`. Injects `ThemeService`. | Interactive light/dark mode switch. |

---

## 3. Template Control Flow Patterns

Templates across the codebase exclusively use modern built-in control flow:

```html
<!-- Example from HeaderComponent -->
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

<!-- Example from ProductListComponent -->
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
```
