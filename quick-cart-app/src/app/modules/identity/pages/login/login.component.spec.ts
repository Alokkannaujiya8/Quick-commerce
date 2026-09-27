import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { LoginComponent } from './login.component';
import { AuthService } from '../../../../core/services/auth.service';
import { GoogleIdentityService } from '../../../../core/auth/google-identity.service';
import { AuthResponse } from '../../../../core/auth/auth.models';
import { signal } from '@angular/core';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let googleIdentitySpy: jasmine.SpyObj<GoogleIdentityService>;
  let router: Router;

  const mockAuthResponse: AuthResponse = {
    user: {
      id: 'u-101',
      fullName: 'Priya Nair',
      email: 'priya@gmail.com',
      emailVerified: true,
    },
    tokens: {
      accessToken: 'jwt-token-101',
      refreshToken: 'refresh-token-101',
    },
  };

  beforeEach(async () => {
    authServiceSpy = jasmine.createSpyObj<AuthService>('AuthService', [
      'sendOtp',
      'googleSignIn',
    ]);
    (authServiceSpy as any).currentUser = signal(null);

    googleIdentitySpy = jasmine.createSpyObj<GoogleIdentityService>(
      'GoogleIdentityService',
      ['requestIdToken']
    );

    await TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: authServiceSpy },
        { provide: GoogleIdentityService, useValue: googleIdentitySpy },
      ],
    }).compileComponents();

    router = TestBed.inject(Router);
    spyOn(router, 'navigate').and.returnValue(Promise.resolve(true));

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create LoginComponent', () => {
    expect(component).toBeTruthy();
  });

  it('should request Google ID token, call authService.googleSignIn, and navigate to home on success', () => {
    googleIdentitySpy.requestIdToken.and.returnValue(of('valid-google-id-token'));
    authServiceSpy.googleSignIn.and.returnValue(of(mockAuthResponse));

    component.onGoogleSignIn();

    expect(googleIdentitySpy.requestIdToken).toHaveBeenCalled();
    expect(authServiceSpy.googleSignIn).toHaveBeenCalledWith('valid-google-id-token');
    expect(router.navigate).toHaveBeenCalledWith(['/']);
    expect(component.isGoogleLoading()).toBeFalse();
  });

  it('should display error banner and not navigate when Google authentication fails', () => {
    googleIdentitySpy.requestIdToken.and.returnValue(of('expired-google-id-token'));
    authServiceSpy.googleSignIn.and.returnValue(
      throwError(() => ({
        error: { detail: 'Invalid or expired Google ID token.' },
      }))
    );

    component.onGoogleSignIn();

    expect(authServiceSpy.googleSignIn).toHaveBeenCalledWith('expired-google-id-token');
    expect(router.navigate).not.toHaveBeenCalled();
    expect(component.isErrorToast()).toBeTrue();
    expect(component.toastMessage()).toBe('Invalid or expired Google ID token.');
    expect(component.isGoogleLoading()).toBeFalse();
  });
});
