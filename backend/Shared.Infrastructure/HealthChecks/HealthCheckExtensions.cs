using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace Shared.Infrastructure.HealthChecks;

/// <summary>
/// Health check configuration for microservices
/// Supports Kubernetes readiness/liveness probes
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Adds health checks with database check
    /// </summary>
    public static IServiceCollection AddServiceHealthChecks<TDbContext>(
        this IServiceCollection services, 
        string serviceName) where TDbContext : DbContext
    {
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy($"{serviceName} is running"))
            .AddDbContextCheck<TDbContext>(
                name: "database",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "db", "sql", "sqlite" });

        return services;
    }

    /// <summary>
    /// Adds health checks without database (for API Gateway)
    /// </summary>
    public static IServiceCollection AddServiceHealthChecks(
        this IServiceCollection services, 
        string serviceName)
    {
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy($"{serviceName} is running"));

        return services;
    }

    /// <summary>
    /// Maps health check endpoints with detailed JSON response
    /// </summary>
    public static IEndpointRouteBuilder MapServiceHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        // Liveness probe - basic check if the service is running
        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Name == "self",
            ResponseWriter = WriteHealthCheckResponse
        });

        // Readiness probe - checks all dependencies
        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = WriteHealthCheckResponse
        });

        // Full health check with all details
        endpoints.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = WriteHealthCheckResponse
        });

        return endpoints;
    }

    private static async Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow.ToString("o"),
            duration = report.TotalDuration.TotalMilliseconds + "ms",
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.TotalMilliseconds + "ms",
                description = e.Value.Description,
                exception = e.Value.Exception?.Message,
                data = e.Value.Data
            })
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        await context.Response.WriteAsJsonAsync(response, options);
    }
}
