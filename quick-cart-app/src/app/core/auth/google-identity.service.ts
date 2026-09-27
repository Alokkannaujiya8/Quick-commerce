import { DOCUMENT } from '@angular/common';
import { Injectable, NgZone, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface GoogleCredentialResponse {
  credential: string;
  select_by?: string;
}

interface GooglePromptMomentNotification {
  isNotDisplayed(): boolean;
  isSkippedMoment(): boolean;
  isDismissedMoment(): boolean;
  getNotDisplayedReason?(): string;
  getSkippedReason?(): string;
  getDismissedReason?(): string;
}

interface GoogleAccountsId {
  initialize(config: {
    client_id: string;
    callback: (response: GoogleCredentialResponse) => void;
    auto_select?: boolean;
    cancel_on_tap_outside?: boolean;
    ux_mode?: 'popup' | 'redirect';
  }): void;
  prompt(momentListener?: (notification: GooglePromptMomentNotification) => void): void;
  renderButton(
    parent: HTMLElement,
    options: {
      type?: 'standard' | 'icon';
      theme?: 'outline' | 'filled_blue' | 'filled_black';
      size?: 'large' | 'medium' | 'small';
    }
  ): void;
}

declare global {
  interface Window {
    google?: {
      accounts?: {
        id?: GoogleAccountsId;
      };
    };
  }
}

@Injectable({
  providedIn: 'root',
})
export class GoogleIdentityService {
  private readonly document = inject(DOCUMENT);
  private readonly ngZone = inject(NgZone);
  private scriptLoadPromise: Promise<void> | null = null;

  requestIdToken(): Observable<string> {
    return new Observable<string>((subscriber) => {
      const clientId = environment.googleClientId?.trim();

      if (!clientId || clientId.startsWith('YOUR_GOOGLE_CLIENT_ID')) {
        subscriber.error(
          new Error(
            'Google Client ID is not configured. Set googleClientId in src/environments/environment.development.ts.'
          )
        );
        return;
      }

      this.ensureGoogleScriptLoaded()
        .then(() => {
          const googleId = window.google?.accounts?.id;
          if (!googleId) {
            subscriber.error(new Error('Google Identity Services SDK failed to initialize.'));
            return;
          }

          let settled = false;

          googleId.initialize({
            client_id: clientId,
            ux_mode: 'popup',
            cancel_on_tap_outside: true,
            callback: (response: GoogleCredentialResponse) => {
              this.ngZone.run(() => {
                if (settled) return;
                settled = true;

                if (response?.credential) {
                  subscriber.next(response.credential);
                  subscriber.complete();
                } else {
                  subscriber.error(new Error('No Google ID token credential was returned.'));
                }
              });
            },
          });

          googleId.prompt((notification: GooglePromptMomentNotification) => {
            this.ngZone.run(() => {
              if (settled) return;

              if (notification.isNotDisplayed() || notification.isSkippedMoment()) {
                // Fallback: trigger a hidden standard Google Sign-In button popup if One Tap is suppressed
                this.triggerFallbackButtonPopup(googleId, (err) => {
                  if (!settled) {
                    settled = true;
                    subscriber.error(err);
                  }
                });
              } else if (notification.isDismissedMoment()) {
                const reason = notification.getDismissedReason?.() || '';
                if (reason !== 'credential_returned') {
                  settled = true;
                  subscriber.error(new Error('Google Sign-In was cancelled.'));
                }
              }
            });
          });
        })
        .catch((err) => {
          this.ngZone.run(() => subscriber.error(err));
        });
    });
  }

  private ensureGoogleScriptLoaded(): Promise<void> {
    if (typeof window !== 'undefined' && window.google?.accounts?.id) {
      return Promise.resolve();
    }

    if (this.scriptLoadPromise) {
      return this.scriptLoadPromise;
    }

    this.scriptLoadPromise = new Promise<void>((resolve, reject) => {
      const existingScript = this.document.querySelector<HTMLScriptElement>(
        'script[src="https://accounts.google.com/gsi/client"]'
      );

      if (existingScript && window.google?.accounts?.id) {
        resolve();
        return;
      }

      const script = existingScript ?? this.document.createElement('script');
      if (!existingScript) {
        script.src = 'https://accounts.google.com/gsi/client';
        script.async = true;
        script.defer = true;
        this.document.head.appendChild(script);
      }

      script.addEventListener('load', () => resolve(), { once: true });
      script.addEventListener(
        'error',
        () => {
          this.scriptLoadPromise = null;
          reject(new Error('Unable to load Google Identity Services script.'));
        },
        { once: true }
      );
    });

    return this.scriptLoadPromise;
  }

  private triggerFallbackButtonPopup(
    googleId: GoogleAccountsId,
    onError: (err: Error) => void
  ): void {
    try {
      const container = this.document.createElement('div');
      container.style.position = 'fixed';
      container.style.opacity = '0';
      container.style.pointerEvents = 'none';
      container.style.top = '-9999px';
      this.document.body.appendChild(container);

      googleId.renderButton(container, {
        type: 'standard',
        theme: 'outline',
        size: 'large',
      });

      const clickable = container.querySelector<HTMLElement>('[role="button"], div[ tabindex="0" ], iframe');
      if (clickable) {
        clickable.click();
      } else {
        onError(
          new Error(
            'Google Sign-In prompt could not be displayed. Check third-party cookie settings or Google Client ID origin configuration.'
          )
        );
      }

      setTimeout(() => {
        container.remove();
      }, 2000);
    } catch {
      onError(new Error('Unable to open Google Sign-In popup.'));
    }
  }
}
