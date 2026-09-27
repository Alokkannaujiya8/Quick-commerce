import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../auth/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getAccessToken();

  const authRequest = token
    ? req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`,
        },
      })
    : req;

  return next(authRequest).pipe(
    catchError((error: unknown) => {
      const isAuthEndpoint =
        req.url.includes('/auth/login') ||
        req.url.includes('/auth/register') ||
        req.url.includes('/auth/google') ||
        req.url.includes('/auth/otp/') ||
        req.url.includes('/auth/refresh');

      if (
        error instanceof HttpErrorResponse &&
        error.status === 401 &&
        !isAuthEndpoint &&
        authService.getRefreshToken()
      ) {
        return authService.refreshToken().pipe(
          switchMap((authResponse) => {
            const newAccessToken = authResponse.tokens?.accessToken || authService.getAccessToken();
            const retriedRequest = newAccessToken
              ? req.clone({
                  setHeaders: {
                    Authorization: `Bearer ${newAccessToken}`,
                  },
                })
              : req;
            return next(retriedRequest);
          })
        );
      }

      return throwError(() => error);
    })
  );
};
