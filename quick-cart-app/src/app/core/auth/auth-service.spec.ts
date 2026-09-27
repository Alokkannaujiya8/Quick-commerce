import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { AuthService } from './auth-service';
import { AuthResponse } from './auth.models';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
      ],
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('googleSignIn() should POST idToken to /api/auth/google and update auth signals on success', () => {
    const mockResponse: AuthResponse = {
      user: {
        id: 'user-google-1',
        fullName: 'Aarav Verma',
        firstName: 'Aarav',
        lastName: 'Verma',
        email: 'aarav@gmail.com',
        emailVerified: true,
        profilePictureUrl: 'https://lh3.googleusercontent.com/a/aarav',
      },
      tokens: {
        accessToken: 'qc-jwt-access-token',
        refreshToken: 'qc-refresh-token',
      },
    };

    let actualResponse: AuthResponse | undefined;
    service.googleSignIn('google-id-token-xyz').subscribe((res) => {
      actualResponse = res;
    });

    const req = httpMock.expectOne((r) => r.url.endsWith('/api/auth/google'));
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ idToken: 'google-id-token-xyz' });
    req.flush(mockResponse);

    expect(actualResponse).toEqual(mockResponse);
    expect(service.currentUser()?.email).toBe('aarav@gmail.com');
    expect(service.user()?.fullName).toBe('Aarav Verma');
    expect(service.isAuthenticated()).toBeTrue();
    expect(service.getAccessToken()).toBe('qc-jwt-access-token');
    expect(service.getRefreshToken()).toBe('qc-refresh-token');
  });

  it('googleSignIn() should propagate 401 error and keep user unauthenticated on failure', () => {
    let errorStatus = 0;

    service.googleSignIn('invalid-google-id-token').subscribe({
      next: () => fail('Expected error'),
      error: (err) => {
        errorStatus = err.status;
      },
    });

    const req = httpMock.expectOne((r) => r.url.endsWith('/api/auth/google'));
    req.flush(
      { detail: 'Invalid or expired Google ID token.' },
      { status: 401, statusText: 'Unauthorized' }
    );

    expect(errorStatus).toBe(401);
    expect(service.currentUser()).toBeNull();
    expect(service.isAuthenticated()).toBeFalse();
  });
});
