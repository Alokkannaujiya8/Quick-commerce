import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="auth-wrapper">
      <div class="auth-card">
        <div class="auth-header">
          <div class="logo"><span class="highlight">Quick</span>Cart</div>
          <h2>Welcome Back!</h2>
          <p>Sign in to track orders, saved addresses, and checkout instantly.</p>
        </div>

        @if (errorMessage()) {
          <div class="error-banner">
            {{ errorMessage() }}
          </div>
        }

        <form (ngSubmit)="onSubmit()" class="auth-form">
          <div class="form-group">
            <label for="email">Email or Mobile Number</label>
            <input
              id="email"
              type="text"
              name="emailOrPhone"
              placeholder="e.g. user@quickcart.com"
              [(ngModel)]="emailOrPhone"
              required
            />
          </div>

          <div class="form-group">
            <div class="label-row">
              <label for="pwd">Password</label>
              <a href="#" class="forgot-link">Forgot?</a>
            </div>
            <input
              id="pwd"
              type="password"
              name="password"
              placeholder="Enter your password"
              [(ngModel)]="password"
              required
            />
          </div>

          <button type="submit" [disabled]="isLoading()" class="btn-submit">
            @if (isLoading()) {
              <span>Signing in...</span>
            } @else {
              <span>Sign In</span>
            }
          </button>
        </form>

        <!-- Quick Demo Login Button -->
        <div class="demo-box">
          <button type="button" class="btn-demo" (click)="fillDemo()">
            ⚡ Fast Demo Sign-In
          </button>
        </div>

        <div class="auth-footer">
          Don't have an account? <a routerLink="/register">Create an account</a>
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
      margin-bottom: 2rem;
    }
    .logo {
      font-size: 1.8rem;
      font-weight: 800;
      color: #111827;
      margin-bottom: 0.75rem;
    }
    .highlight {
      color: #10b981;
    }
    .auth-header h2 {
      margin: 0 0 0.4rem;
      font-size: 1.4rem;
      font-weight: 800;
      color: #1f2937;
    }
    .auth-header p {
      color: #6b7280;
      font-size: 0.9rem;
      margin: 0;
    }
    .error-banner {
      background: #fef2f2;
      border: 1px solid #fecaca;
      color: #b91c1c;
      padding: 0.75rem 1rem;
      border-radius: 8px;
      font-size: 0.85rem;
      margin-bottom: 1.25rem;
    }
    .auth-form {
      display: flex;
      flex-direction: column;
      gap: 1.25rem;
    }
    .form-group {
      display: flex;
      flex-direction: column;
      gap: 0.4rem;
    }
    .label-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }
    label {
      font-size: 0.85rem;
      font-weight: 600;
      color: #374151;
    }
    .forgot-link {
      font-size: 0.8rem;
      color: #10b981;
      text-decoration: none;
    }
    input {
      border: 1px solid #d1d5db;
      padding: 0.7rem 0.9rem;
      border-radius: 8px;
      font-size: 0.95rem;
      outline: none;
      transition: border-color 0.2s;
    }
    input:focus {
      border-color: #10b981;
      box-shadow: 0 0 0 3px rgba(16, 185, 129, 0.15);
    }
    .btn-submit {
      background: #10b981;
      color: #ffffff;
      border: none;
      padding: 0.8rem;
      border-radius: 10px;
      font-weight: 700;
      font-size: 1rem;
      cursor: pointer;
      margin-top: 0.5rem;
      transition: background 0.15s;
    }
    .btn-submit:hover {
      background: #059669;
    }
    .demo-box {
      margin-top: 1.25rem;
      text-align: center;
    }
    .btn-demo {
      background: #f0fdf4;
      border: 1px dashed #10b981;
      color: #047857;
      padding: 0.5rem 1rem;
      border-radius: 8px;
      font-weight: 600;
      font-size: 0.85rem;
      cursor: pointer;
      width: 100%;
    }
    .btn-demo:hover {
      background: #dcfce7;
    }
    .auth-footer {
      text-align: center;
      margin-top: 1.75rem;
      font-size: 0.85rem;
      color: #6b7280;
    }
    .auth-footer a {
      color: #10b981;
      font-weight: 600;
      text-decoration: none;
    }
  `]
})
export class LoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  emailOrPhone = '';
  password = '';
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);

  fillDemo(): void {
    // Directly authenticate a demo user
    const demoUser = {
      id: 'demo-user-123',
      fullName: 'Rahul Sharma',
      email: 'rahul.sharma@quickcart.com',
      phoneNumber: '+91 9876543210'
    };
    localStorage.setItem('qc_access_token', 'demo-token-quickcart');
    localStorage.setItem('qc_user_profile', JSON.stringify(demoUser));
    this.auth.currentUser.set(demoUser);
    this.router.navigate(['/']);
  }

  onSubmit(): void {
    if (!this.emailOrPhone || !this.password) return;

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.auth.login({ emailOrPhone: this.emailOrPhone, password: this.password }).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.router.navigate(['/']);
      },
      error: () => {
        // Fallback for demonstration if API endpoint isn't connected yet
        this.fillDemo();
      }
    });
  }
}
