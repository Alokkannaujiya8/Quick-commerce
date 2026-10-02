namespace Identity.Infrastructure;

using System.Text;
using Identity.Application.Interfaces;
using Identity.Infrastructure.Authentication;
using Identity.Infrastructure.Repositories;
using Identity.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();
        services.AddScoped<IIdentityService, IdentityService>();
        return services;
    }

    public static IServiceCollection AddIdentityAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var googleClientId = configuration["Authentication:Google:ClientId"]
            ?? configuration["GoogleAuth:ClientId"]
            ?? string.Empty;
        var allowDevTokens = bool.TryParse(configuration["Authentication:Google:AllowDevSimulatedTokens"], out var parsedAllowDev) && parsedAllowDev;

        services.Configure<GoogleAuthenticationOptions>(options =>
        {
            options.ClientId = googleClientId;
            options.AllowDevSimulatedTokens = allowDevTokens;
        });

        var jwtSection = configuration.GetSection("Jwt");
        var secretKey = jwtSection["SecretKey"]
            ?? JwtTokenService.DefaultSecretKey;
        var issuer = jwtSection["Issuer"] ?? "QuickCart";
        var audience = jwtSection["Audience"] ?? "QuickCart.Client";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();

        return services;
    }
}
