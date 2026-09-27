import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, catchError, throwError } from 'rxjs';
import {
  AuthResponse,
  GoogleAuthRequest,
  LoginRequest,
  RegisterRequest,
  SendOtpResponse,
  User,
} from './auth.models';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly apiUrl = `${environment.apiUrl || 'https://localhost:7189/api'}/auth`;

  private readonly accessTokenKey = 'qc_access_token';
  private readonly refreshTokenKey = 'qc_refresh_token';
  private readonly userKey = 'qc_user_profile';

  readonly currentUser = signal<User | null>(this.getStoredUser());
  readonly user = this.currentUser.asReadonly();
  readonly isAuthenticated = computed(() => !!this.currentUser() || !!this.getAccessToken());
  readonly pendingPhoneNumber = signal<string>('');

  sendOtp(phoneNumber: string): Observable<SendOtpResponse> {
    return this.http
      .post<SendOtpResponse>(`${this.apiUrl}/otp/send`, { phoneNumber })
      .pipe(tap(() => this.pendingPhoneNumber.set(phoneNumber)));
  }

  verifyOtp(phoneNumber: string, code: string): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.apiUrl}/otp/verify`, { phoneNumber, code })
      .pipe(
        tap((response) => {
          this.pendingPhoneNumber.set('');
          this.setSession(response);
        })
      );
  }

  googleSignIn(idToken: string): Observable<AuthResponse> {
    const payload: GoogleAuthRequest = { idToken };
    return this.http
      .post<AuthResponse>(`${this.apiUrl}/google`, payload)
      .pipe(tap((response) => this.setSession(response)));
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.apiUrl}/register`, request)
      .pipe(tap((response) => this.setSession(response)));
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    const payload = {
      login: request.login || request.email || request.emailOrPhone || '',
      password: request.password,
      deviceName: request.deviceName || 'Web Browser',
    };
    return this.http
      .post<AuthResponse>(`${this.apiUrl}/login`, payload)
      .pipe(tap((response) => this.setSession(response)));
  }

  refreshToken(): Observable<AuthResponse> {
    const storedRefreshToken = this.getRefreshToken();
    return this.http
      .post<AuthResponse>(`${this.apiUrl}/refresh`, {
        refreshToken: storedRefreshToken,
        deviceName: 'Web Browser',
      })
      .pipe(
        tap((response) => this.setSession(response)),
        catchError((error) => {
          this.clearSession();
          return throwError(() => error);
        })
      );
  }

  getMe(): Observable<User> {
    return this.http.get<User>(`${this.apiUrl}/me`).pipe(
      tap((user) => {
        localStorage.setItem(this.userKey, JSON.stringify(user));
        this.currentUser.set(user);
      })
    );
  }

  logout(): void {
    const refreshToken = this.getRefreshToken();
    if (refreshToken) {
      this.http.post<void>(`${this.apiUrl}/logout`, { refreshToken }).subscribe({
        error: () => {},
      });
    }
    this.clearSession();
    this.router.navigate(['/login']);
  }

  getAccessToken(): string | null {
    return localStorage.getItem(this.accessTokenKey) || localStorage.getItem('accessToken');
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.refreshTokenKey) || localStorage.getItem('refreshToken');
  }

  getToken(): string | null {
    return this.getAccessToken();
  }

  private setSession(response: AuthResponse): void {
    const accessToken =
      response.tokens?.accessToken ||
      (typeof response.token === 'string' ? response.token : response.token?.accessToken) ||
      '';
    const refreshToken =
      response.tokens?.refreshToken ||
      (typeof response.token === 'object' ? response.token?.refreshToken : '') ||
      '';

    if (accessToken) {
      localStorage.setItem(this.accessTokenKey, accessToken);
      localStorage.setItem('accessToken', accessToken);
    }
    if (refreshToken) {
      localStorage.setItem(this.refreshTokenKey, refreshToken);
      localStorage.setItem('refreshToken', refreshToken);
    }
    if (response.user) {
      localStorage.setItem(this.userKey, JSON.stringify(response.user));
      this.currentUser.set(response.user);
    }
  }

  private clearSession(): void {
    localStorage.removeItem(this.accessTokenKey);
    localStorage.removeItem(this.refreshTokenKey);
    localStorage.removeItem(this.userKey);
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    this.currentUser.set(null);
  }

  private getStoredUser(): User | null {
    try {
      const stored = localStorage.getItem(this.userKey);
      return stored ? JSON.parse(stored) : null;
    } catch {
      return null;
    }
  }
}
