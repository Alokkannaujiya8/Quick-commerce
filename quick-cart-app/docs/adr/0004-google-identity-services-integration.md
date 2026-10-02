# ADR 0004: Native Google Identity Services (GIS) with Single-Initialization Popup

## Status
**Accepted**

## Context
Google deprecated legacy Google Sign-In (`gapi.auth2`) in favor of Google Identity Services (`google.accounts.id`). Initial implementations of GIS often trigger console warnings (`[GSI_LOGGER]: google.accounts.id.initialize() is called multiple times.`) or attempt to invoke deprecated One Tap UI status callbacks (`isNotDisplayed`, `isSkippedMoment`, `isDismissedMoment`) which throw runtime warnings or fail on modern browsers with strict third-party cookie blocking.

## Decision
Implement a dedicated `GoogleIdentityService` (`src/app/core/auth/google-identity.service.ts`) that:
1. Dynamically injects the GIS script tag (`https://accounts.google.com/gsi/client`) asynchronously only when needed.
2. Guards initialization with an `initialized` flag so `google.accounts.id.initialize()` is called exactly once per application lifecycle.
3. Uses standard `ux_mode: 'popup'` and official GIS button rendering (including hidden container trigger when programmatic click is needed).
4. Does not call deprecated One Tap UI methods or status callbacks.
5. Re-enters the Angular execution zone via `this.ngZone.run()` when GIS callbacks return outside Angular.
6. Provides local development fallback token emission when configured with local dev client IDs.

## Consequences
### Positive
* Zero console warnings or deprecation errors.
* Guaranteed Zone.js change detection upon token return.
* Clean RxJS Observable interface (`requestIdToken(): Observable<string>`).
* Fully unit-testable using Jasmine spies.

### Negative / Trade-offs
* Requires third-party script loading from Google CDN at runtime.
