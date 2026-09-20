using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;

namespace Public.Api.Middlerware
{
    public class ApiRateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiRateLimitMiddleware> _logger;

        private readonly ConcurrentDictionary<
            string,
            FixedWindowRateLimiter> _limiters = new();

        public ApiRateLimitMiddleware(
            RequestDelegate next,
            ILogger<ApiRateLimitMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(
            HttpContext context,
            IConfiguration configuration)
        {
            // =====================================================
            // Configuration
            // =====================================================

            var permitLimit =
                configuration.GetValue<int?>(
                    "ApiRateLimit:PermitLimit"
                ) ?? 60;

            var windowSeconds =
                configuration.GetValue<int?>(
                    "ApiRateLimit:WindowSeconds"
                ) ?? 60;

            // =====================================================
            // Determine Rate Limit Key
            //
            // 1. API Token if available
            // 2. Otherwise client IP
            // =====================================================

            var rateLimitKey =
                GetRateLimitKey(context);

            // =====================================================
            // Get/Create limiter for this caller
            // =====================================================

            var limiter =
                _limiters.GetOrAdd(
                    rateLimitKey,
                    _ =>
                        new FixedWindowRateLimiter(
                            new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = permitLimit,

                                Window =
                                    TimeSpan.FromSeconds(
                                        windowSeconds
                                    ),

                                QueueProcessingOrder =
                                    QueueProcessingOrder.OldestFirst,

                                QueueLimit = 0,

                                AutoReplenishment = true
                            }
                        )
                );

            // =====================================================
            // Acquire request permit
            // =====================================================

            using var lease =
                await limiter.AcquireAsync(
                    permitCount: 1,
                    cancellationToken:
                        context.RequestAborted
                );

            // =====================================================
            // Allowed
            // =====================================================

            if (lease.IsAcquired)
            {
                await _next(context);
                return;
            }

            // =====================================================
            // Rate Limit Exceeded
            // =====================================================

            _logger.LogWarning(
                "Rate limit exceeded. Path: {Path}",
                context.Request.Path
            );

            context.Response.StatusCode =
                StatusCodes.Status429TooManyRequests;

            context.Response.ContentType =
                "application/json";

            int? retryAfterSeconds = null;

            if (lease.TryGetMetadata(
                MetadataName.RetryAfter,
                out var retryAfter))
            {
                retryAfterSeconds =
                    Math.Max(
                        1,
                        (int)Math.Ceiling(
                            retryAfter.TotalSeconds
                        )
                    );

                context.Response.Headers.RetryAfter =
                    retryAfterSeconds.Value.ToString();
            }

            await context.Response.WriteAsJsonAsync(new
            {
                received = true,
                success = false,
                statusCode = 429,

                message =
                    "Rate limit exceeded. Please try again later.",

                data = new
                {
                    limit = permitLimit,
                    windowSeconds,
                    retryAfterSeconds
                },

                errors = Array.Empty<string>(),

                timestamp = DateTime.Now
            });
        }

        // =========================================================
        // Rate Limit Partition Key
        // =========================================================

        private static string GetRateLimitKey(
            HttpContext context)
        {
            var authorizationHeader =
                context.Request.Headers["Authorization"]
                    .FirstOrDefault();

            // =====================================================
            // API Token available
            // =====================================================

            if (!string.IsNullOrWhiteSpace(
                    authorizationHeader) &&
                authorizationHeader.StartsWith(
                    "Bearer ",
                    StringComparison.OrdinalIgnoreCase))
            {
                var token =
                    authorizationHeader
                        .Substring("Bearer ".Length)
                        .Trim();

                if (!string.IsNullOrWhiteSpace(token))
                {
                    // Never store/use raw token as dictionary key.
                    var tokenHash =
                        Convert.ToHexString(
                            SHA256.HashData(
                                Encoding.UTF8.GetBytes(token)
                            )
                        );

                    return $"TOKEN:{tokenHash}";
                }
            }

            // =====================================================
            // No API Token
            //
            // Login / Health Check / other public endpoints
            // are limited by IP address.
            // =====================================================

            var ipAddress =
                context.Connection.RemoteIpAddress?
                    .ToString();

            if (!string.IsNullOrWhiteSpace(ipAddress))
            {
                return $"IP:{ipAddress}";
            }

            // Extremely defensive fallback
            return "IP:UNKNOWN";
        }
    }
}