# External Integrations Architecture

This document catalogs and details the verified external third-party integrations and protocols implemented across the QuickCart backend, as well as unconfirmed/unimplemented integration capabilities.

---

## 1. Verified Integrations Summary

| Integration | Technology / Provider | Hosting Module | Current Implementation Status |
| :--- | :--- | :--- | :--- |
| **Google Sign-In** | Google OpenID Connect JWKS (`Google.Apis.Auth` `1.76.0`) | `Identity` | **Verified & Active** |
| **Payment Gateway** | HMAC-SHA256 Signature Verification / Webhooks | `Payment` | **Verified & Active** |
| **Real-Time Telemetry** | ASP.NET Core SignalR WebSockets | `Delivery` | **Verified & Active** |
| **SMS / OTP Provider** | External SMS Gateway (e.g., Twilio, AWS SNS) | `Identity` | `Status: Not Implemented / Not Confirmed` |
| **Email Gateway** | Transactional Email (e.g., SendGrid, SMTP) | `Identity` | `Status: Not Implemented / Not Confirmed` |
| **Message Broker** | Kafka, RabbitMQ, Azure Service Bus | Architecture | `Status: Not Implemented / Not Confirmed` |

---

## 2. Granular Integration Details

### 2.1 Google OpenID Connect Authentication
* **Component**: [`GoogleTokenValidator`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/Modules/Identity/Identity.Infrastructure/Authentication/GoogleTokenValidator.cs)
* **Library**: `Google.Apis.Auth` (`1.76.0`)
* **Verification Pipeline**:
  * Direct cryptographic validation against Google's public key JWKS endpoints using `GoogleJsonWebSignature.ValidateAsync`.
  * Verifies issuer matches `accounts.google.com` or `https://accounts.google.com`.
  * Enforces audience matching against `Authentication:Google:ClientId`.
  * Applies $\pm 2$ minute clock skew tolerance for timestamp validation.
  * Rejects unverified email accounts (`EmailVerified == false`).
  * Development mode supports simulated test tokens (`dev_google_id_token:*`) when `Authentication:Google:AllowDevSimulatedTokens` is set to `true`.

### 2.2 Payment Gateway Adapter
* **Component**: [`PaymentGatewayService`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/Modules/Payment/Payment.Infrastructure/Services/PaymentGatewayService.cs)
* **Security & Verification**:
  * Generates and verifies HMAC-SHA256 digests over order and payment payload pairs (`${providerOrderId}|${providerPaymentId}`).
  * Performs constant-time byte comparisons (`CryptographicOperations.FixedTimeEquals`) to prevent timing attack vulnerabilities.
  * Inbound server-to-server webhook verification via the `X-Payment-Signature` header.
  * Idempotent deduplication through `payment.PaymentWebhookEvents` based on unique `ProviderEventId` and SHA-256 payload digest.

### 2.3 SignalR Telemetry Hub
* **Component**: [`DeliveryTrackingHub`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/Modules/Delivery/Delivery.Presentation/Hubs/DeliveryTrackingHub.cs)
* **Route**: `/hubs/delivery-tracking`
* **Protocol**: WebSockets with fallback to Server-Sent Events / Long Polling.
* **Capabilities**:
  * Room-based subscriptions (`JoinOrderTrackingGroup`, `LeaveOrderTrackingGroup`).
  * Real-time order state broadcasts (`ReceiveOrderStatusUpdate`).
  * High-frequency rider location and dynamic ETA streaming (`ReceiveRiderLocationUpdate`).

---

## 3. Unimplemented Third-Party Integrations

### 3.1 External SMS & Email Gateways
```text
Status: Not Implemented / Not Confirmed

Evidence:
1. Inspected src/Modules/Identity/Identity.Infrastructure/Services/IdentityService.cs. Methods SendOtpAsync and VerifyOtpAsync handle OTP generation in-memory and write diagnostic log entries. No HTTP client or SDK integration for SMS providers (such as Twilio, AWS SNS, Karix) or Email providers (such as SendGrid, AWS SES, Mailgun) is implemented.
```

### 3.2 External Message Brokers (Event-Driven Architecture)
```text
Status: Not Implemented / Not Confirmed

Evidence:
1. Inspected all project files (.csproj) and dependency registrations. No references to MassTransit, RabbitMQ.Client, Confluent.Kafka, or Azure.Messaging.ServiceBus exist.
2. Inter-module communication executes strictly in-process through strongly-typed Application contracts.
```

