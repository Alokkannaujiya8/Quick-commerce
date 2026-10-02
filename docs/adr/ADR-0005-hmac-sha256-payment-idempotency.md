# ADR-0005: Server-Verified Payment Intents, HMAC-SHA256 & Idempotency

## Status
Approved

## Context
High-concurrency e-commerce checkouts are susceptible to client-side price tampering, double-charging on network retries, and timing attacks on signature verification. Financial integrity requires strict server-side verification, cryptographic validation, and idempotency guarantees.

## Decision
1. **Server-Side Price Authority**: Client requests never dictate the payable amount. `PaymentService` queries `IOrderService.GetOrderByIdAsync` to establish the authoritative order total from the database before generating a payment intent.
2. **Idempotency Enforcement**: Payment intents enforce a unique `IdempotencyKey`. Replay attempts return the existing active payment record without creating duplicate transactions.
3. **Constant-Time HMAC-SHA256 Signatures**: Gateway signatures are verified using `HMACSHA256` compared via `CryptographicOperations.FixedTimeEquals` to prevent side-channel timing attacks.
4. **Webhook Deduplication**: Webhooks are validated and deduplicated against `payment.PaymentWebhookEvents` based on unique `ProviderEventId` and SHA-256 payload digest.
5. **Auditing**: Every payment status change generates an immutable record in `payment.PaymentAuditLogs`.

## Consequences
### Positive
* Prevents payment fraud via client-side manipulation.
* Eliminates accidental duplicate charges from user double-clicks or mobile network reconnects.
* Guarantees audit compliance for payment lifecycle events.

### Negative / Trade-offs
* Cross-module contract invocation adds an in-process call from `Payment` to `Ordering`.
* Database insertions required for idempotency keys and audit records.

## Verification & Code References
* Entities: [`Payment`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/Modules/Payment/Payment.Domain/Entities/Payment.cs), [`PaymentAuditLog`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/Modules/Payment/Payment.Domain/Entities/PaymentAuditLog.cs), [`PaymentWebhookEvent`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/Modules/Payment/Payment.Domain/Entities/PaymentWebhookEvent.cs)
* Services: [`PaymentService`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/Modules/Payment/Payment.Infrastructure/Services/PaymentService.cs), [`PaymentGatewayService`](file:///d:/Asp.net%20core_Project/Quick-commerce/src/Modules/Payment/Payment.Infrastructure/Services/PaymentGatewayService.cs)
* Tests: [`PaymentServiceTests`](file:///d:/Asp.net%20core_Project/Quick-commerce/tests/Payment.Tests/Services/PaymentServiceTests.cs)

