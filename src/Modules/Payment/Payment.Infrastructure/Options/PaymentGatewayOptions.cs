namespace Payment.Infrastructure.Options;

public sealed class PaymentGatewayOptions
{
    public const string SectionName = "PaymentGateway";

    public string ProviderName { get; set; } = "QuickCartPay";
    public string KeyId { get; set; } = "qcp_test_key_local";
    public string WebhookSecret { get; set; } = "QuickCart_Dev_Webhook_HMAC_Secret_2026_Min32Bytes!";
    public bool ExposeDevSignature { get; set; } = false;
}
