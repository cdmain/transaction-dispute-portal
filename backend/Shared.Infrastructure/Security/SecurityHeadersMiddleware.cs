using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Shared.Infrastructure.Security;

/// <summary>
/// Middleware to add comprehensive security headers to all responses
/// Implements OWASP 2025 security recommendations
/// Protects against XSS, clickjacking, MIME sniffing, and other attacks
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SecurityHeadersOptions _options;

    public SecurityHeadersMiddleware(RequestDelegate next, SecurityHeadersOptions? options = null)
    {
        _next = next;
        _options = options ?? new SecurityHeadersOptions();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Prevent XSS attacks by enabling browser's XSS filter (legacy, but still useful)
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

        // Prevent clickjacking by disallowing framing
        context.Response.Headers.Append("X-Frame-Options", _options.FrameOptions);

        // Prevent MIME type sniffing
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

        // Content Security Policy - restrict resource loading
        context.Response.Headers.Append("Content-Security-Policy", _options.ContentSecurityPolicy);

        // Referrer Policy - control referrer information
        context.Response.Headers.Append("Referrer-Policy", _options.ReferrerPolicy);

        // Permissions Policy - control browser features (replaces Feature-Policy)
        context.Response.Headers.Append("Permissions-Policy", _options.PermissionsPolicy);

        // Strict Transport Security - force HTTPS (only in production)
        if (_options.EnableHsts)
        {
            context.Response.Headers.Append("Strict-Transport-Security", 
                $"max-age={_options.HstsMaxAge}; includeSubDomains; preload");
        }

        // Cross-Origin policies for isolation
        context.Response.Headers.Append("Cross-Origin-Embedder-Policy", _options.CrossOriginEmbedderPolicy);
        context.Response.Headers.Append("Cross-Origin-Opener-Policy", _options.CrossOriginOpenerPolicy);
        context.Response.Headers.Append("Cross-Origin-Resource-Policy", _options.CrossOriginResourcePolicy);

        // Remove server header to hide technology stack
        context.Response.Headers.Remove("Server");
        context.Response.Headers.Remove("X-Powered-By");
        context.Response.Headers.Remove("X-AspNet-Version");
        context.Response.Headers.Remove("X-AspNetMvc-Version");

        await _next(context);
    }
}

/// <summary>
/// Options for configuring security headers
/// </summary>
public class SecurityHeadersOptions
{
    /// <summary>
    /// X-Frame-Options header value. Default: DENY
    /// </summary>
    public string FrameOptions { get; set; } = "DENY";

    /// <summary>
    /// Content-Security-Policy header value
    /// </summary>
    public string ContentSecurityPolicy { get; set; } = 
        "default-src 'self'; " +
        "script-src 'self'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data: https:; " +
        "font-src 'self'; " +
        "connect-src 'self'; " +
        "frame-ancestors 'none'; " +
        "base-uri 'self'; " +
        "form-action 'self'; " +
        "upgrade-insecure-requests;";

    /// <summary>
    /// Referrer-Policy header value. Default: strict-origin-when-cross-origin
    /// </summary>
    public string ReferrerPolicy { get; set; } = "strict-origin-when-cross-origin";

    /// <summary>
    /// Permissions-Policy header value
    /// </summary>
    public string PermissionsPolicy { get; set; } = 
        "accelerometer=(), " +
        "camera=(), " +
        "geolocation=(), " +
        "gyroscope=(), " +
        "magnetometer=(), " +
        "microphone=(), " +
        "payment=(), " +
        "usb=(), " +
        "interest-cohort=()";

    /// <summary>
    /// Whether to enable HSTS. Should be true in production.
    /// </summary>
    public bool EnableHsts { get; set; } = false;

    /// <summary>
    /// HSTS max-age in seconds. Default: 1 year
    /// </summary>
    public int HstsMaxAge { get; set; } = 31536000;

    /// <summary>
    /// Cross-Origin-Embedder-Policy. Default: require-corp
    /// </summary>
    public string CrossOriginEmbedderPolicy { get; set; } = "credentialless";

    /// <summary>
    /// Cross-Origin-Opener-Policy. Default: same-origin
    /// </summary>
    public string CrossOriginOpenerPolicy { get; set; } = "same-origin";

    /// <summary>
    /// Cross-Origin-Resource-Policy. Default: same-origin
    /// </summary>
    public string CrossOriginResourcePolicy { get; set; } = "same-origin";
}

/// <summary>
/// Extension methods for adding security headers middleware
/// </summary>
public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>();
    }

    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder, SecurityHeadersOptions options)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>(options);
    }

    /// <summary>
    /// Adds API-specific security headers (more permissive CSP for API responses)
    /// </summary>
    public static IApplicationBuilder UseApiSecurityHeaders(this IApplicationBuilder builder)
    {
        var options = new SecurityHeadersOptions
        {
            ContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none';",
            CrossOriginEmbedderPolicy = "credentialless",
            CrossOriginResourcePolicy = "cross-origin" // Allow API to be consumed cross-origin
        };
        return builder.UseMiddleware<SecurityHeadersMiddleware>(options);
    }
}
