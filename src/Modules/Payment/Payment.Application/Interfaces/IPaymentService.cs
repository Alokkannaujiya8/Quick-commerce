namespace Payment.Application.Interfaces;

using Payment.Application.DTOs;

public interface IPaymentService
{
    Task<PaymentIntentResponse> CreatePaymentIntentAsync(
        Guid userId,
        CreatePaymentIntentRequest request,
        CancellationToken cancellationToken = default);

    Task<PaymentDto> VerifyPaymentAsync(
        Guid userId,
        VerifyPaymentRequest request,
        CancellationToken cancellationToken = default);

    Task<WebhookProcessResult> ProcessWebhookAsync(
        string rawPayload,
        string signatureHeader,
        PaymentWebhookPayload payload,
        CancellationToken cancellationToken = default);

    Task<PaymentDto?> GetPaymentByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);

    Task<PaymentDto?> GetPaymentByIdAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default);

    Task<PaymentIntentResponse> RetryPaymentAsync(
        Guid userId,
        Guid paymentId,
        CancellationToken cancellationToken = default);

    Task<PaymentDto> RefundPaymentAsync(
        Guid paymentId,
        RefundPaymentRequest request,
        CancellationToken cancellationToken = default);
}
