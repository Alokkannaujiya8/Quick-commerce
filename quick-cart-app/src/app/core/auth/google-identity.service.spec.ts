import { TestBed } from '@angular/core/testing';
import { GoogleCredentialResponse, GoogleIdentityService } from './google-identity.service';

describe('GoogleIdentityService', () => {
  let service: GoogleIdentityService;
  let initializeSpy: jasmine.Spy;
  let renderButtonSpy: jasmine.Spy;
  let capturedCallback: ((response: GoogleCredentialResponse) => void) | undefined;

  beforeEach(() => {
    capturedCallback = undefined;

    initializeSpy = jasmine.createSpy('initialize').and.callFake((config: any) => {
      capturedCallback = config.callback;
    });

    renderButtonSpy = jasmine.createSpy('renderButton').and.callFake((parent: HTMLElement) => {
      const btn = document.createElement('div');
      btn.setAttribute('role', 'button');
      parent.appendChild(btn);
    });

    (window as any).google = {
      accounts: {
        id: {
          initialize: initializeSpy,
          renderButton: renderButtonSpy,
        },
      },
    };

    TestBed.configureTestingModule({});
    service = TestBed.inject(GoogleIdentityService);
  });

  afterEach(() => {
    delete (window as any).google;
  });

  it('should call google.accounts.id.initialize only once when initializeGoogle is called multiple times', () => {
    service.initializeGoogle();
    service.initializeGoogle();
    service.initializeGoogle();

    expect(initializeSpy).toHaveBeenCalledTimes(1);
  });

  it('should call google.accounts.id.initialize only once across multiple requestIdToken invocations', async () => {
    const sub1 = service.requestIdToken().subscribe();
    await Promise.resolve();
    sub1.unsubscribe();

    const sub2 = service.requestIdToken().subscribe();
    await Promise.resolve();
    sub2.unsubscribe();

    expect(initializeSpy).toHaveBeenCalledTimes(1);
    expect(renderButtonSpy).toHaveBeenCalledTimes(1);
  });

  it('should emit credential when Google callback is invoked', async () => {
    let emittedToken: string | undefined;

    service.requestIdToken().subscribe((token) => {
      emittedToken = token;
    });

    await Promise.resolve();
    expect(capturedCallback).toBeDefined();

    capturedCallback!({ credential: 'real-google-id-token-abc' });

    expect(emittedToken).toBe('real-google-id-token-abc');
  });
});
