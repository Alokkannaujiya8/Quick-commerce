# QuickCart Frontend — Observability Specification

This document details the implemented observability, telemetry, and logging capabilities in the QuickCart frontend, along with an explicit accounting of unconfirmed or unintegrated monitoring features.

---

## 1. Implemented Observability Capabilities

### 1.1 Browser Global Error Listening
* **Implementation**: Registered in `src/app/app.config.ts` via `provideBrowserGlobalErrorListeners()`.
* **Behavior**: Catches uncaught runtime exceptions occurring in asynchronous callbacks, event handlers, and change detection ticks, routing them to standard browser error dispatchers.

### 1.2 API Client Error Logging
* **Implementation**: In `src/app/core/services/api.service.ts`:
  ```typescript
  private handleError(error: unknown) {
    console.error('API Error:', error);
    return throwError(() => error);
  }
  ```
* **Behavior**: Logs failed HTTP calls with full error metadata to the browser developer console prior to re-throwing.

### 1.3 Service Fallback Warnings
* **Implementation**: In `OrderService` and `CheckoutComponent`:
  ```typescript
  console.warn('Backend orders/checkout unreachable, falling back to local order state:', err);
  console.warn('Payment API step fallback:', err);
  ```
* **Behavior**: Emits warning logs when network calls to backend services fail and fallbacks are engaged.

---

## 2. Unsupported / Unimplemented Capabilities

### 2.1 Remote Telemetry & APM (Application Performance Monitoring)
```text
Status: Not Implemented / Not Confirmed

Evidence:
Inspected package.json, src/app/app.config.ts, and src/environments/*. No client SDKs for Sentry, Application Insights, Datadog, LogRocket, or OpenTelemetry are installed or configured.
```

### 2.2 Client-Side User Analytics
```text
Status: Not Implemented / Not Confirmed

Evidence:
Inspected package.json and src/main.ts. No Google Analytics (GA4), Mixpanel, Segment, or PostHog tracking libraries or custom tracking services exist in the frontend code.
```

### 2.3 Distributed Correlation & Request IDs
```text
Status: Not Implemented / Not Confirmed

Evidence:
Inspected src/app/core/interceptors/auth.interceptor.ts and src/app/core/services/api.service.ts. Neither X-Correlation-ID nor TraceParent headers are injected into outgoing HTTP requests.
```
