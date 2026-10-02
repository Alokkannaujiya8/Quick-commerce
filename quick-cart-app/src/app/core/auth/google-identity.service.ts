import { DOCUMENT } from '@angular/common';
import { Injectable, NgZone, inject } from '@angular/core';
import { Observable, Subscriber } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface GoogleCredentialResponse {
  credential: string;
  select_by?: string;
}

export interface GoogleAccountsId {
  initialize(config: {
    client_id: string;
    callback: (response: GoogleCredentialResponse) => void;
    auto_select?: boolean;
    cancel_on_tap_outside?: boolean;
    ux_mode?: 'popup' | 'redirect';
  }): void;
  renderButton(
    parent: HTMLElement,
    options: {
      type?: 'standard' | 'icon';
      theme?: 'outline' | 'filled_blue' | 'filled_black';
      size?: 'large' | 'medium' | 'small';
      text?: 'signin_with' | 'signup_with' | 'continue_with' | 'signin';
      shape?: 'rectangular' | 'pill' | 'circle' | 'square';
      width?: number;
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
  private initialized = false;
  private activeSubscriber: Subscriber<string> | null = null;
  private customCredentialCallback: ((response: GoogleCredentialResponse) => void) | null = null;
  private hiddenButtonContainer: HTMLElement | null = null;

  /**
   * Initializes Google Identity Services at most once per browser application lifecycle.
   * Subsequent calls are no-ops to prevent "[GSI_LOGGER]: google.accounts.id.initialize() is called multiple times."
   */
  initializeGoogle(callback?: (response: GoogleCredentialResponse) => void): void {
    if (callback) {
      this.customCredentialCallback = callback;
    }

    if (this.initialized) {
      return;
    }

    const clientId = environment.googleClientId?.trim();
    const googleId = typeof window !== 'undefined' ? window.google?.accounts?.id : undefined;

    if (!googleId || !clientId || clientId.startsWith('YOUR_GOOGLE_CLIENT_ID')) {
      return;
    }

    googleId.initialize({
      client_id: clientId,
      ux_mode: 'popup',
      auto_select: false,
      cancel_on_tap_outside: true,
      callback: (response: GoogleCredentialResponse) => {
        this.ngZone.run(() => {
          this.handleCredentialResponse(response);
        });
      },
    });

    this.initialized = true;
  }

  /**
   * Renders the official Google Sign-In button inside the provided element after ensuring single initialization.
   */
  renderButton(element: HTMLElement): void {
    const googleId = typeof window !== 'undefined' ? window.google?.accounts?.id : undefined;
    if (!googleId) {
      return;
    }

    this.initializeGoogle();
    googleId.renderButton(element, {
      theme: 'outline',
      size: 'large',
      type: 'standard',
      text: 'continue_with',
    });
  }

  /**
   * Requests a Google ID token using the single-initialized GIS button flow without calling deprecated One Tap UI status methods.
   */
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

      // Local development simulated Google Sign-In when using the local dev Client ID
      if (!environment.production && clientId.startsWith('dev-local-quickcart-client')) {
        this.ngZone.run(() => {
          subscriber.next(
            'dev_google_id_token:google-dev-sub-1001:alok.quickcart@gmail.com:Alok:Kannaujiya'
          );
          subscriber.complete();
        });
        return;
      }

      this.activeSubscriber = subscriber;

      this.ensureGoogleScriptLoaded()
        .then(() => {
          const googleId = window.google?.accounts?.id;
          if (!googleId) {
            this.activeSubscriber = null;
            subscriber.error(new Error('Google Identity Services SDK failed to initialize.'));
            return;
          }

          // Initialize Google Identity Services ONCE per lifecycle
          this.initializeGoogle();

          // Open the standard GIS button popup directly without calling google.accounts.id.prompt()
          // or any deprecated One Tap UI status methods (isNotDisplayed, isSkippedMoment, isDismissedMoment)
          this.triggerRenderedButtonPopup(googleId, (err) => {
            if (this.activeSubscriber === subscriber) {
              this.activeSubscriber = null;
              subscriber.error(err);
            }
          });
        })
        .catch((err) => {
          this.ngZone.run(() => {
            if (this.activeSubscriber === subscriber) {
              this.activeSubscriber = null;
            }
            subscriber.error(err);
          });
        });

      return () => {
        if (this.activeSubscriber === subscriber) {
          this.activeSubscriber = null;
        }
      };
    });
  }

  private handleCredentialResponse(response: GoogleCredentialResponse): void {
    if (this.customCredentialCallback) {
      this.customCredentialCallback(response);
    }

    const subscriber = this.activeSubscriber;
    this.activeSubscriber = null;

    if (!subscriber) {
      return;
    }

    if (response?.credential) {
      subscriber.next(response.credential);
      subscriber.complete();
    } else {
      subscriber.error(new Error('No Google ID token credential was returned.'));
    }
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

  private triggerRenderedButtonPopup(
    googleId: GoogleAccountsId,
    onError: (err: Error) => void
  ): void {
    try {
      if (!this.hiddenButtonContainer || !this.document.body.contains(this.hiddenButtonContainer)) {
        const container = this.document.createElement('div');
        container.setAttribute('aria-hidden', 'true');
        container.style.position = 'fixed';
        container.style.opacity = '0';
        container.style.pointerEvents = 'none';
        container.style.top = '-9999px';
        container.style.left = '-9999px';
        this.document.body.appendChild(container);

        googleId.renderButton(container, {
          type: 'standard',
          theme: 'outline',
          size: 'large',
          text: 'continue_with',
        });

        this.hiddenButtonContainer = container;
      }

      const clickable = this.hiddenButtonContainer.querySelector<HTMLElement>(
        '[role="button"], div[tabindex="0"], iframe'
      );
      if (clickable) {
        clickable.click();
      } else {
        onError(
          new Error(
            'Google Sign-In button could not be initialized. Check your network connection or Google Client ID origin configuration.'
          )
        );
      }
    } catch {
      onError(new Error('Unable to open Google Sign-In popup.'));
    }
  }
}
