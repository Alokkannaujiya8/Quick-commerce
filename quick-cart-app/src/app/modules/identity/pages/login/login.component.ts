import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Observable, of, switchMap } from 'rxjs';
import { AuthService } from '../../../../core/services/auth.service';
import { GoogleIdentityService } from '../../../../core/auth/google-identity.service';
import {
  AlertBannerComponent,
  AuthCardShellComponent,
  UiButtonComponent,
} from '../../../../shared/components';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    FormsModule,
    RouterLink,
    AuthCardShellComponent,
    AlertBannerComponent,
    UiButtonComponent,
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent {
  private readonly auth = inject(AuthService);
  private readonly googleIdentity = inject(GoogleIdentityService);
  private readonly router = inject(Router);

  readonly phoneNumber = signal<string>('');
  readonly isInputFocused = signal<boolean>(false);
  readonly isLoading = signal<boolean>(false);
  readonly isGoogleLoading = signal<boolean>(false);
  readonly toastMessage = signal<string | null>(null);
  readonly isErrorToast = signal<boolean>(false);

  onPhoneChange(value: string): void {
    const digitsOnly = (value || '').replace(/\D/g, '').slice(0, 10);
    this.phoneNumber.set(digitsOnly);
    this.toastMessage.set(null);
  }

  isValidPhone(): boolean {
    return /^\d{10}$/.test(this.phoneNumber());
  }

  onContinue(): void {
    if (!this.isValidPhone() || this.isLoading()) return;

    this.isLoading.set(true);
    this.toastMessage.set(null);

    const phone = this.phoneNumber();
    this.auth.sendOtp(phone).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.router.navigate(['/login/verify'], { queryParams: { phone } });
      },
      error: (err: any) => {
        this.isLoading.set(false);
        this.isErrorToast.set(true);
        const msg =
          err?.error?.detail ||
          err?.error?.message ||
          'Unable to send OTP. Ensure the QuickCart.Api backend is running.';
        this.toastMessage.set(msg);
      },
    });
  }

  onGoogleSignIn(directIdToken?: string): void {
    if (this.isGoogleLoading()) return;

    this.isGoogleLoading.set(true);
    this.toastMessage.set(null);
    this.isErrorToast.set(false);

    const tokenSource$: Observable<string> = directIdToken
      ? of(directIdToken)
      : this.googleIdentity.requestIdToken();

    tokenSource$
      .pipe(switchMap((idToken) => this.auth.googleSignIn(idToken)))
      .subscribe({
        next: () => {
          this.isGoogleLoading.set(false);
          this.router.navigate(['/']);
        },
        error: (err: any) => {
          this.isGoogleLoading.set(false);
          this.isErrorToast.set(true);
          const msg =
            err?.error?.detail ||
            err?.error?.message ||
            (err?.status === 401
              ? 'Google account could not be verified.'
              : err?.message || 'Google sign-in failed. Please try again.');
          this.toastMessage.set(msg);
        },
      });
  }
}
