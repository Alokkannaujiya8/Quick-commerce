namespace QuickCart.Api.Bootstrap;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

public static class MiddlewarePipeline
{
    public static WebApplication UseApplicationPipeline(this WebApplication app)
    {
        // 1. Global RFC 7807 Exception Handling
        app.UseExceptionHandler();

        // 2. Interactive OpenAPI & API Documentation
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(options =>
            {
                options.Title = "QuickCart API Documentation";
                options.Theme = ScalarTheme.Purple;
            });
        }

        // 3. Security & Transport
        app.UseHttpsRedirection();
        app.UseRouting();

        // 4. CORS
        app.UseCors(DependencyInjection.FrontendCorsPolicy);

        // 5. Auth
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}

