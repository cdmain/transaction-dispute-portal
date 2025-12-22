using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Shared.Infrastructure.Middleware;

/// <summary>
/// Global exception handler that returns consistent ProblemDetails responses.
/// Implements RFC 7807 Problem Details for HTTP APIs.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var correlationId = httpContext.GetCorrelationId() ?? Guid.NewGuid().ToString("N");

        _logger.LogError(exception, 
            "Unhandled exception occurred. CorrelationId: {CorrelationId}, Path: {Path}", 
            correlationId, 
            httpContext.Request.Path);

        var problemDetails = CreateProblemDetails(httpContext, exception, correlationId);

        httpContext.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private ProblemDetails CreateProblemDetails(HttpContext context, Exception exception, string correlationId)
    {
        var (statusCode, title, type) = exception switch
        {
            ArgumentNullException => (StatusCodes.Status400BadRequest, "Invalid Request", "https://tools.ietf.org/html/rfc7231#section-6.5.1"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid Argument", "https://tools.ietf.org/html/rfc7231#section-6.5.1"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized", "https://tools.ietf.org/html/rfc7235#section-3.1"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource Not Found", "https://tools.ietf.org/html/rfc7231#section-6.5.4"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Conflict", "https://tools.ietf.org/html/rfc7231#section-6.5.8"),
            OperationCanceledException => (StatusCodes.Status499ClientClosedRequest, "Client Closed Request", "https://httpstatuses.com/499"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error", "https://tools.ietf.org/html/rfc7231#section-6.6.1")
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = type,
            Instance = context.Request.Path,
            Detail = _environment.IsDevelopment() ? exception.Message : "An error occurred processing your request."
        };

        // Add correlation ID for tracking
        problemDetails.Extensions["correlationId"] = correlationId;
        problemDetails.Extensions["timestamp"] = DateTime.UtcNow.ToString("o");

        // Include stack trace in development
        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            problemDetails.Extensions["exceptionType"] = exception.GetType().Name;
        }

        return problemDetails;
    }
}

/// <summary>
/// Extension method for registering the global exception handler
/// </summary>
public static class GlobalExceptionHandlerExtensions
{
    public static IServiceCollection AddGlobalExceptionHandler(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        return services;
    }
}
