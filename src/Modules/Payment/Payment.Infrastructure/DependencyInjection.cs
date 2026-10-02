namespace Payment.Infrastructure;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payment.Application.Interfaces;
using Payment.Infrastructure.Options;
using Payment.Infrastructure.Repositories;
using Payment.Infrastructure.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration.GetSection(PaymentGatewayOptions.SectionName);
        services.Configure<PaymentGatewayOptions>(options =>
        {
            options.ProviderName = section[nameof(PaymentGatewayOptions.ProviderName)] ?? options.ProviderName;
            options.KeyId = section[nameof(PaymentGatewayOptions.KeyId)] ?? options.KeyId;
            options.WebhookSecret = section[nameof(PaymentGatewayOptions.WebhookSecret)] ?? options.WebhookSecret;
            if (bool.TryParse(section[nameof(PaymentGatewayOptions.ExposeDevSignature)], out var exposeDevSig))
            {
                options.ExposeDevSignature = exposeDevSig;
            }
        });

        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IPaymentGatewayService, PaymentGatewayService>();
        services.AddScoped<IPaymentService, PaymentService>();

        return services;
    }
}
