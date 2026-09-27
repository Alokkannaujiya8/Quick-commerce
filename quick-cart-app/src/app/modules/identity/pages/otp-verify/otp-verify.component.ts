import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import {
  AlertBannerComponent,
  AuthCardShellComponent,
  UiButtonComponent,
} from '../../../../shared/components';

@Component({
  selector: 'app-otp-verify',
  standalone: true,
  imports: [
    FormsModule,
    AuthCardShellComponent,
    AlertBannerComponent,
    UiButtonComponent,
  ],
  templateUrl: './otp-verify.component.html',
  styleUrl: './otp-verify.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OtpVerifyComponent implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly phoneNumber = signal<string>('');
  readonly otpCode = signal<string>('');
  readonly isLoading = signal<boolean>(false);
  readonly isResending = signal<boolean>(false);
  readonly errorMessage = signal<string | null>(null);
  readonly infoMessage = signal<string | null>(null);

  ngOnInit(): void {
    const queryPhone = this.route.snapshot.queryParamMap.get('phone');
    const pendingPhone = this.auth.pendingPhoneNumber();
    const resolvedPhone = queryPhone || pendingPhone || '';

    if (!resolvedPhone) {
      this.router.navigate(['/login']);
      return;
    }

    this.phoneNumber.set(resolvedPhone);
  }

  onOtpInput(value: string): void {
    const digits = (value || '').replace(/\D/g, '').slice(0, 6);
    this.otpCode.set(digits);
    this.errorMessage.set(null);
  }

  isOtpValid(): boolean {
    return /^\d{6}$/.test(this.otpCode());
  }

  onVerify(): void {
    if (!this.isOtpValid() || this.isLoading()) return;

    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.infoMessage.set(null);

    this.auth.verifyOtp(this.phoneNumber(), this.otpCode()).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.router.navigate(['/']);
      },
      error: (err: any) => {
        this.isLoading.set(false);
        const msg =
          err?.error?.detail ||
          err?.error?.message ||
          'Invalid or expired OTP code. Please check the console log and try again.';
        this.errorMessage.set(msg);
      },
    });
  }

  onResendOtp(): void {
    if (this.isResending() || !this.phoneNumber()) return;

    this.isResending.set(true);
    this.errorMessage.set(null);
    this.infoMessage.set(null);

    this.auth.sendOtp(this.phoneNumber()).subscribe({
      next: () => {
        this.isResending.set(false);
        this.infoMessage.set('A new 6-digit OTP has been generated in the backend console.');
      },
      error: () => {
        this.isResending.set(false);
        this.errorMessage.set('Failed to resend OTP. Please try again.');
      },
    });
  }
}
