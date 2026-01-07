using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Shared.Infrastructure.Security;

/// <summary>
/// Middleware for detecting and logging suspicious request patterns
/// Implements OWASP 2025 security monitoring recommendations
/// </summary>
public class RequestValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestValidationMiddleware> _logger;
    private readonly RequestValidationOptions _options;

    public RequestValidationMiddleware(
        RequestDelegate next, 
        ILogger<RequestValidationMiddleware> logger,
        RequestValidationOptions? options = null)
    {
        _next = next;
        _logger = logger;
        _options = options ?? new RequestValidationOptions();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Validate request size
        if (context.Request.ContentLength > _options.MaxRequestSizeBytes)
        {
            _logger.LogWarning(
                "Request too large from {IP}: {Size} bytes (max: {Max})",
                context.Connection.RemoteIpAddress,
                context.Request.ContentLength,
                _options.MaxRequestSizeBytes);

            context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
            await WriteErrorResponse(context, "Request payload too large");
            return;
        }

        // Validate Content-Type for POST/PUT/PATCH
        if (HttpMethods.IsPost(context.Request.Method) ||
            HttpMethods.IsPut(context.Request.Method) ||
            HttpMethods.IsPatch(context.Request.Method))
        {
            var contentType = context.Request.ContentType;
            if (!string.IsNullOrEmpty(contentType) && 
                !_options.AllowedContentTypes.Any(ct => contentType.StartsWith(ct, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning(
                    "Invalid Content-Type from {IP}: {ContentType}",
                    context.Connection.RemoteIpAddress,
                    contentType);

                // Don't block, just log - some legitimate clients may use unusual content types
            }
        }

        // Check for suspicious patterns in query string
        var query = context.Request.QueryString.Value;
        if (!string.IsNullOrEmpty(query))
        {
            if (InputSanitizer.ContainsSqlInjection(query))
            {
                _logger.LogWarning(
                    "Potential SQL injection in query string from {IP}: {Query}",
                    context.Connection.RemoteIpAddress,
                    query.Length > 200 ? query[..200] + "..." : query);

                if (_options.BlockSuspiciousRequests)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await WriteErrorResponse(context, "Invalid request parameters");
                    return;
                }
            }

            if (InputSanitizer.ContainsXss(query))
            {
                _logger.LogWarning(
                    "Potential XSS in query string from {IP}: {Query}",
                    context.Connection.RemoteIpAddress,
                    query.Length > 200 ? query[..200] + "..." : query);

                if (_options.BlockSuspiciousRequests)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await WriteErrorResponse(context, "Invalid request parameters");
                    return;
                }
            }
        }

        // Check for suspicious User-Agent patterns
        var userAgent = context.Request.Headers.UserAgent.ToString();
        if (_options.ValidateUserAgent && IsSuspiciousUserAgent(userAgent))
        {
            _logger.LogWarning(
                "Suspicious User-Agent from {IP}: {UserAgent}",
                context.Connection.RemoteIpAddress,
                userAgent.Length > 200 ? userAgent[..200] + "..." : userAgent);
        }

        await _next(context);
    }

    private static bool IsSuspiciousUserAgent(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
            return true;

        var suspicious = new[] { "sqlmap", "nikto", "nmap", "masscan", "dirbuster", "gobuster" };
        return suspicious.Any(s => userAgent.Contains(s, StringComparison.OrdinalIgnoreCase));
    }

    private static async Task WriteErrorResponse(HttpContext context, string message)
    {
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            title = "Bad Request",
            status = context.Response.StatusCode,
            detail = message,
            instance = context.Request.Path.ToString()
        });
    }
}

/// <summary>
/// Options for request validation middleware
/// </summary>
public class RequestValidationOptions
{
    /// <summary>
    /// Maximum request size in bytes. Default: 10MB
    /// </summary>
    public long MaxRequestSizeBytes { get; set; } = 10 * 1024 * 1024;

    /// <summary>
    /// Allowed Content-Type prefixes for body requests
    /// </summary>
    public string[] AllowedContentTypes { get; set; } = new[]
    {
        "application/json",
        "application/x-www-form-urlencoded",
        "multipart/form-data",
        "text/plain"
    };

    /// <summary>
    /// Whether to block requests with suspicious patterns
    /// </summary>
    public bool BlockSuspiciousRequests { get; set; } = true;

    /// <summary>
    /// Whether to validate User-Agent headers
    /// </summary>
    public bool ValidateUserAgent { get; set; } = true;
}

/// <summary>
/// Extension methods for request validation middleware
/// </summary>
public static class RequestValidationMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestValidation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestValidationMiddleware>();
    }

    public static IApplicationBuilder UseRequestValidation(this IApplicationBuilder builder, RequestValidationOptions options)
    {
        return builder.UseMiddleware<RequestValidationMiddleware>(options);
    }
}
