namespace Payment.Infrastructure.Services;

using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Payment.Application.Interfaces;
using Payment.Infrastructure.Options;

public sealed class PaymentGatewayService : IPaymentGatewayService
{
    private readonly PaymentGatewayOptions _options;

    public PaymentGatewayService(IOptions<PaymentGatewayOptions> options)
    {
        _options = options.Value;
    }

    public Task<GatewayOrderCreationResult> CreateGatewayOrderAsync(
        Guid orderId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default)
    {
        var providerOrderId = $"order_{Guid.NewGuid():N}"[..26];
        var simulatedPaymentId = $"pay_{orderId:N}"[..20];
        var devSignature = _options.ExposeDevSignature
            ? ComputeCheckoutSignature(providerOrderId, simulatedPaymentId)
            : null;

        var result = new GatewayOrderCreationResult(
            _options.ProviderName,
            providerOrderId,
            _options.KeyId,
            devSignature);

        return Task.FromResult(result);
    }

    public bool VerifyPaymentSignature(
        string providerOrderId,
        string providerPaymentId,
        string providerSignature)
    {
        if (string.IsNullOrWhiteSpace(providerOrderId) ||
            string.IsNullOrWhiteSpace(providerPaymentId) ||
            string.IsNullOrWhiteSpace(providerSignature))
        {
            return false;
        }

        var expected = ComputeCheckoutSignature(providerOrderId.Trim(), providerPaymentId.Trim());
        return FixedTimeEqualsHex(expected, providerSignature.Trim());
    }

    public bool VerifyWebhookSignature(string rawPayload, string signatureHeader)
    {
        if (string.IsNullOrWhiteSpace(rawPayload) || string.IsNullOrWhiteSpace(signatureHeader))
        {
            return false;
        }

        var expected = ComputeWebhookSignature(rawPayload);
        return FixedTimeEqualsHex(expected, signatureHeader.Trim());
    }

    public string ComputeCheckoutSignature(string providerOrderId, string providerPaymentId)
    {
        var payload = $"{providerOrderId}|{providerPaymentId}";
        return ComputeHmacSha256(payload, _options.WebhookSecret);
    }

    public string ComputeWebhookSignature(string rawPayload)
    {
        return ComputeHmacSha256(rawPayload, _options.WebhookSecret);
    }

    public Task<string> ProcessRefundAsync(
        string providerPaymentId,
        decimal amount,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var refundId = $"rfnd_{Guid.NewGuid():N}"[..21];
        return Task.FromResult(refundId);
    }

    private static string ComputeHmacSha256(string data, string secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
            throw new InvalidOperationException("Payment gateway webhook secret is not configured.");

        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var dataBytes = Encoding.UTF8.GetBytes(data);
        var hashBytes = HMACSHA256.HashData(keyBytes, dataBytes);
        return Convert.ToHexStringLower(hashBytes);
    }

    private static bool FixedTimeEqualsHex(string expectedHex, string providedHex)
    {
        var expectedBytes = Encoding.UTF8.GetBytes(expectedHex.ToLowerInvariant());
        var providedBytes = Encoding.UTF8.GetBytes(providedHex.ToLowerInvariant());
        return CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
    }
}
