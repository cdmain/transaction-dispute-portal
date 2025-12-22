using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Infrastructure.RateLimiting;

/// <summary>
/// Rate limiting configuration for .NET 8
/// Provides protection against abuse and DDoS attacks
/// </summary>
public static class RateLimitingExtensions
{
    public const string FixedPolicy = "fixed";
    public const string SlidingPolicy = "sliding";
    public const string TokenBucketPolicy = "token";
    public const string ConcurrencyPolicy = "concurrency";

    /// <summary>
    /// Adds rate limiting with sensible defaults for a production API
    /// </summary>
    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Return 429 Too Many Requests with Retry-After header
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.ContentType = "application/problem+json";
                
                var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retry) 
                    ? retry.TotalSeconds 
                    : 60;

                context.HttpContext.Response.Headers.RetryAfter = retryAfter.ToString();

                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    type = "https://tools.ietf.org/html/rfc6585#section-4",
                    title = "Too Many Requests",
                    status = 429,
                    detail = $"Rate limit exceeded. Please retry after {retryAfter} seconds.",
                    instance = context.HttpContext.Request.Path.ToString(),
                    retryAfterSeconds = retryAfter
                }, token);
            };

            // Fixed window: 100 requests per minute per client
            options.AddFixedWindowLimiter(FixedPolicy, opt =>
            {
                opt.PermitLimit = 100;
                opt.Window = TimeSpan.FromMinutes(1);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 10;
            });

            // Sliding window: smoother rate limiting (60 requests per minute)
            options.AddSlidingWindowLimiter(SlidingPolicy, opt =>
            {
                opt.PermitLimit = 60;
                opt.Window = TimeSpan.FromMinutes(1);
                opt.SegmentsPerWindow = 6; // 10-second segments
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 5;
            });

            // Token bucket: allows bursts (10 tokens, refill 2 per second)
            options.AddTokenBucketLimiter(TokenBucketPolicy, opt =>
            {
                opt.TokenLimit = 10;
                opt.ReplenishmentPeriod = TimeSpan.FromSeconds(1);
                opt.TokensPerPeriod = 2;
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 5;
            });

            // Concurrency limiter: max 10 concurrent requests
            options.AddConcurrencyLimiter(ConcurrencyPolicy, opt =>
            {
                opt.PermitLimit = 10;
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 5;
            });

            // Global policy based on client IP
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: clientIp,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 200,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 20
                    });
            });
        });

        return services;
    }
}
