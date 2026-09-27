namespace Identity.Infrastructure;

using System.Text;
using Identity.Application.Interfaces;
using Identity.Infrastructure.Authentication;
using Identity.Infrastructure.Persistence;
using Identity.Infrastructure.Repositories;
using Identity.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        if (configuration is not null)
        {
            var connectionString = configuration.GetConnectionString("QuickCartDb")
                ?? configuration.GetConnectionString("QuickCart");

            if (!string.IsNullOrEmpty(connectionString))
            {
                services.AddDbContext<IdentityDbContext>(options =>
                {
                    options.UseNpgsql(
                        connectionString,
                        npgsql =>
                        {
                            npgsql.MigrationsHistoryTable(
                                "__EFMigrationsHistory",
                                "identity");
                        });
                });
            }

            services.Configure<GoogleAuthenticationOptions>(
                configuration.GetSection(GoogleAuthenticationOptions.SectionName));
        }
        else
        {
            services.Configure<GoogleAuthenticationOptions>(_ => { });
        }

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        return services;
    }

    public static IServiceCollection AddIdentityAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var secretKey = jwtSection["SecretKey"]
            ?? "QuickCart_Default_Jwt_Secret_Key_At_Least_32_Bytes_Long_2026!";
        var issuer = jwtSection["Issuer"] ?? "QuickCart";
        var audience = jwtSection["Audience"] ?? "QuickCart.Client";

        var keyBytes = Encoding.UTF8.GetBytes(secretKey);

        services.Configure<GoogleAuthenticationOptions>(
            configuration.GetSection(GoogleAuthenticationOptions.SectionName));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            };
        });

        services.AddAuthorization();

        return services;
    }
}
