import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { LoginRequest } from '../../../../core/models/user.model';
import {
  AlertBannerComponent,
  AuthCardShellComponent,
  UiButtonComponent,
} from '../../../../shared/components';

@Component({
  selector: 'app-password-login',
  standalone: true,
  imports: [
    FormsModule,
    RouterLink,
    AuthCardShellComponent,
    AlertBannerComponent,
    UiButtonComponent,
  ],
  templateUrl: './password-login.component.html',
  styleUrl: './password-login.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PasswordLoginComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  credentials: LoginRequest = {
    email: '',
    password: '',
  };

  onSubmit(): void {
    if (!this.credentials.email || !this.credentials.password) {
      this.errorMessage.set('Please enter both your email and password.');
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.authService.login(this.credentials).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.router.navigate(['/']);
      },
      error: (err: any) => {
        this.isLoading.set(false);
        const msg =
          err?.error?.detail ||
          err?.error?.message ||
          'Invalid email or password. Please try again.';
        this.errorMessage.set(msg);
      },
    });
  }
}
