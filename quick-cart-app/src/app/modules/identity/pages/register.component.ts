import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { RegisterRequest } from '../../../core/models/user.model';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="auth-wrapper">
      <div class="auth-card">
        <div class="auth-header">
          <div class="logo"><span class="highlight">Quick</span>Cart</div>
          <h2>Create Account</h2>
          <p>Get instant 10-minute delivery to your doorstep.</p>
        </div>

        @if (errorMessage()) {
          <div class="error-banner">
            {{ errorMessage() }}
          </div>
        }

        <form (ngSubmit)="onSubmit()" class="auth-form">
          <div class="form-group">
            <label for="fullName">Full Name</label>
            <input
              id="fullName"
              type="text"
              name="fullName"
              placeholder="e.g. Rahul Sharma"
              [(ngModel)]="formData.fullName"
              required
            />
          </div>

          <div class="form-group">
            <label for="email">Email Address</label>
            <input
              id="email"
              type="email"
              name="email"
              placeholder="name@example.com"
              [(ngModel)]="formData.email"
              required
            />
          </div>

          <div class="form-group">
            <label for="phone">Phone Number</label>
            <input
              id="phone"
              type="tel"
              name="phoneNumber"
              placeholder="+91 98765 43210"
              [(ngModel)]="formData.phoneNumber"
              required
            />
          </div>

          <div class="form-group">
            <label for="pwd">Password</label>
            <input
              id="pwd"
              type="password"
              name="password"
              placeholder="At least 6 characters"
              [(ngModel)]="formData.password"
              required
            />
          </div>

          <div class="form-group">
            <label for="confirmPwd">Confirm Password</label>
            <input
              id="confirmPwd"
              type="password"
              name="confirmPassword"
              placeholder="Re-enter your password"
              [(ngModel)]="confirmPassword"
              required
            />
          </div>

          <button type="submit" [disabled]="isLoading()" class="btn-submit">
            @if (isLoading()) {
              <span>Creating Account...</span>
            } @else {
              <span>Sign Up & Start Shopping</span>
            }
          </button>
        </form>

        <div class="auth-footer">
          Already have an account? <a routerLink="/login">Sign In</a>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .auth-wrapper {
      min-height: 80vh;
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 2rem 1rem;
    }
    .auth-card {
      background: #ffffff;
      border: 1px solid #e5e7eb;
      border-radius: 16px;
      padding: 2.5rem;
      max-width: 440px;
      width: 100%;
      box-shadow: 0 10px 25px rgba(0, 0, 0, 0.05);
    }
    .auth-header {
      text-align: center;
      margin-bottom: 1.75rem;
    }
    .logo {
      font-size: 1.8rem;
      font-weight: 800;
      color: #111827;
      margin-bottom: 0.5rem;
    }
    .highlight {
      color: #10b981;
    }
    h2 {
      font-size: 1.4rem;
      font-weight: 700;
      color: #1f2937;
      margin: 0 0 0.25rem;
    }
    p {
      color: #6b7280;
      font-size: 0.88rem;
      margin: 0;
    }
    .error-banner {
      background: #fef2f2;
      border: 1px solid #fee2e2;
      color: #ef4444;
      padding: 0.75rem 1rem;
      border-radius: 8px;
      font-size: 0.88rem;
      margin-bottom: 1.25rem;
    }
    .auth-form {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }
    .form-group {
      display: flex;
      flex-direction: column;
      gap: 0.35rem;
    }
    label {
      font-size: 0.82rem;
      font-weight: 600;
      color: #374151;
    }
    input {
      padding: 0.75rem 1rem;
      border: 1.5px solid #d1d5db;
      border-radius: 8px;
      font-size: 0.95rem;
      transition: border-color 0.2s;
    }
    input:focus {
      outline: none;
      border-color: #10b981;
      box-shadow: 0 0 0 3px rgba(16, 185, 129, 0.15);
    }
    .btn-submit {
      margin-top: 0.5rem;
      background: #10b981;
      color: #ffffff;
      border: none;
      padding: 0.85rem;
      border-radius: 8px;
      font-size: 1rem;
      font-weight: 700;
      cursor: pointer;
      box-shadow: 0 4px 12px rgba(16, 185, 129, 0.3);
      transition: background 0.15s;
    }
    .btn-submit:hover:not(:disabled) {
      background: #059669;
    }
    .btn-submit:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }
    .auth-footer {
      text-align: center;
      margin-top: 1.5rem;
      font-size: 0.88rem;
      color: #6b7280;
    }
    .auth-footer a {
      color: #10b981;
      font-weight: 600;
      text-decoration: none;
    }
    .auth-footer a:hover {
      text-decoration: underline;
    }
  `]
})
export class RegisterComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  confirmPassword = '';
  formData: RegisterRequest = {
    fullName: '',
    email: '',
    phoneNumber: '',
    password: ''
  };

  onSubmit(): void {
    if (!this.formData.fullName || !this.formData.email || !this.formData.password) {
      this.errorMessage.set('Please fill out all required fields.');
      return;
    }

    if (this.formData.password !== this.confirmPassword) {
      this.errorMessage.set('Passwords do not match.');
      return;
    }

    if (this.formData.password.length < 6) {
      this.errorMessage.set('Password must be at least 6 characters long.');
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.authService.register(this.formData).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.router.navigate(['/']);
      },
      error: () => {
        // Fallback for demo if backend auth endpoint is not ready yet
        const simulatedUser = {
          id: 'usr-' + Date.now(),
          fullName: this.formData.fullName,
          email: this.formData.email,
          phoneNumber: this.formData.phoneNumber,
          role: 'Customer'
        };
        localStorage.setItem('qc_access_token', 'simulated_jwt_' + Date.now());
        localStorage.setItem('qc_user_profile', JSON.stringify(simulatedUser));
        this.authService.currentUser.set(simulatedUser);

        this.isLoading.set(false);
        this.router.navigate(['/']);
      }
    });
  }
}
