using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Shared.Infrastructure.Middleware;

/// <summary>
/// Middleware that adds correlation IDs to requests for distributed tracing.
/// If a correlation ID is provided in the request header, it will be used.
/// Otherwise, a new GUID will be generated.
/// </summary>
public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Try to get correlation ID from header, or generate a new one
        var correlationId = GetOrCreateCorrelationId(context);

        // Add correlation ID to response headers
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.TryAdd(CorrelationIdHeader, correlationId);
            return Task.CompletedTask;
        });

        // Store in HttpContext.Items for access throughout the request
        context.Items["CorrelationId"] = correlationId;

        // Add to logging scope so all logs include it
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["RequestPath"] = context.Request.Path.ToString()
        }))
        {
            _logger.LogDebug("Processing request with CorrelationId: {CorrelationId}", correlationId);
            await _next(context);
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out StringValues correlationId) 
            && !string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId.ToString();
        }

        return Guid.NewGuid().ToString("N");
    }
}

/// <summary>
/// Extension methods for CorrelationId middleware
/// </summary>
public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CorrelationIdMiddleware>();
    }

    /// <summary>
    /// Get correlation ID from HttpContext
    /// </summary>
    public static string? GetCorrelationId(this HttpContext context)
    {
        return context.Items.TryGetValue("CorrelationId", out var correlationId) 
            ? correlationId?.ToString() 
            : null;
    }
}
