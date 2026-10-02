# Forms & Validation Architecture Specification

This document details the forms architecture, template-driven binding patterns, input sanitization routines, and client-side validation mechanisms implemented across the QuickCart frontend.

---

## 1. Forms Architecture

* **Module**: Uses `@angular/forms` (`FormsModule`) with two-way data binding (`[(ngModel)]`).
* **Approach**: **Template-Driven Forms**. Preferred for its lightweight bundle footprint and direct compatibility with component-level signals.
* **Reactive Feedback**: Validation errors and submission states are bound to Angular Signals (`errorMessage`, `isSubmitting`, `isLoading`).

---

## 2. Input Sanitization & Masking Patterns

### 2.1 Mobile Phone Sanitization (`LoginComponent`)
Quick commerce requires strict 10-digit phone numbers for SMS OTP delivery. The input handler strips non-digit characters and truncates to 10 characters in real time:
```typescript
onPhoneChange(value: string): void {
  const digitsOnly = (value || '').replace(/\D/g, '').slice(0, 10);
  this.phoneNumber.set(digitsOnly);
  this.toastMessage.set(null);
}

isValidPhone(): boolean {
  return /^\d{10}$/.test(this.phoneNumber());
}
```

### 2.2 OTP Code Sanitization (`OtpVerifyComponent`)
The verification code input enforces exact 6-digit numeric sequences:
```typescript
onOtpInput(value: string): void {
  const digits = (value || '').replace(/\D/g, '').slice(0, 6);
  this.otpCode.set(digits);
  this.errorMessage.set(null);
}

isOtpValid(): boolean {
  return /^\d{6}$/.test(this.otpCode());
}
```

---

## 3. Form Implementations by Feature

### 3.1 Registration Form (`RegisterComponent`)
* **Fields**: `fullName`, `phoneNumber`, `email`, `password`, `confirmPassword`.
* **Validation Rules**:
  * All fields required.
  * `password === confirmPassword`.
  * `password.length >= 6`.
* **Error Display**: Triggers `errorMessage.set('Passwords do not match.')` and displays via `AlertBannerComponent`.

### 3.2 Password Login Form (`PasswordLoginComponent`)
* **Fields**: `email`, `password`.
* **Validation Rules**: Both fields non-empty before dispatching HTTP request.

### 3.3 Checkout Form (`CheckoutComponent`)
* **Fields**: Shipping address (`fullName`, `phoneNumber`, `streetAddress`, `city`, `state`, `postalCode`), `selectedPayment` (`'UPI'` | `'Card'` | `'COD'`).
* **Validation Rules**:
  ```typescript
  if (!this.address.fullName || !this.address.phoneNumber || !this.address.streetAddress) {
    this.errorMessage.set(
      'Please fill in your recipient name, phone number, and delivery street address.'
    );
    return;
  }
  ```
* **Auto-population**: In `ngOnInit()`, if `authService.currentUser()` is set, the form automatically pre-fills `fullName` and `phoneNumber`.

---

## 4. UI Error Feedback

Validation feedback uses:
1. `AlertBannerComponent` (`src/app/shared/components/alert-banner/`): Displays dismissible danger banners.
2. In-place validation styling on inputs: Form controls toggle error borders and helper text based on validation state.
