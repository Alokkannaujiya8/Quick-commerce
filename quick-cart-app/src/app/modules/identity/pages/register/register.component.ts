import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { RegisterRequest } from '../../../../core/models/user.model';
import {
  AlertBannerComponent,
  AuthCardShellComponent,
  UiButtonComponent,
} from '../../../../shared/components';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    FormsModule,
    RouterLink,
    AuthCardShellComponent,
    AlertBannerComponent,
    UiButtonComponent,
  ],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
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
    password: '',
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
      error: (err: any) => {
        this.isLoading.set(false);
        const msg =
          err?.error?.detail ||
          err?.error?.message ||
          'Registration failed. Please check your details and try again.';
        this.errorMessage.set(msg);
      },
    });
  }
}
