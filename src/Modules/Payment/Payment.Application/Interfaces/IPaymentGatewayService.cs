namespace Payment.Application.Interfaces;

public record GatewayOrderCreationResult(
    string ProviderName,
    string ProviderOrderId,
    string ProviderKeyId,
    string? DevVerificationSignature);

public interface IPaymentGatewayService
{
    Task<GatewayOrderCreationResult> CreateGatewayOrderAsync(
        Guid orderId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default);

    bool VerifyPaymentSignature(
        string providerOrderId,
        string providerPaymentId,
        string providerSignature);

    bool VerifyWebhookSignature(
        string rawPayload,
        string signatureHeader);

    string ComputeCheckoutSignature(
        string providerOrderId,
        string providerPaymentId);

    string ComputeWebhookSignature(string rawPayload);

    Task<string> ProcessRefundAsync(
        string providerPaymentId,
        decimal amount,
        string reason,
        CancellationToken cancellationToken = default);
}
