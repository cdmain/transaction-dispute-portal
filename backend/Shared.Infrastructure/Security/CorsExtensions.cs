using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Shared.Infrastructure.Security;

/// <summary>
/// Secure CORS configuration for production environments
/// Implements OWASP 2025 recommendations for cross-origin security
/// </summary>
public static class CorsExtensions
{
    private const string ProductionPolicy = "ProductionCors";
    private const string DevelopmentPolicy = "DevelopmentCors";

    /// <summary>
    /// Adds secure CORS configuration with environment-specific origins
    /// </summary>
    public static IServiceCollection AddSecureCors(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
            ?? new[] { "http://localhost:3000" };

        services.AddCors(options =>
        {
            // Production policy - strict origins, limited methods
            options.AddPolicy(ProductionPolicy, builder =>
            {
                builder
                    .WithOrigins(allowedOrigins)
                    .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
                    .WithHeaders(
                        "Content-Type", 
                        "Authorization", 
                        "X-Requested-With",
                        "X-Correlation-Id",
                        "Accept",
                        "Accept-Language"
                    )
                    .SetPreflightMaxAge(TimeSpan.FromMinutes(10))
                    .DisallowCredentials(); // Prefer token-based auth over cookies
            });

            // Development policy - more permissive for local development
            options.AddPolicy(DevelopmentPolicy, builder =>
            {
                builder
                    .SetIsOriginAllowed(_ => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        return services;
    }

    /// <summary>
    /// Uses the appropriate CORS policy based on environment
    /// </summary>
    public static IApplicationBuilder UseSecureCors(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        var policyName = env.IsDevelopment() ? DevelopmentPolicy : ProductionPolicy;
        return app.UseCors(policyName);
    }
}
