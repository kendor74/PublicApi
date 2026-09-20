namespace Public.Api.Middlerware
{
    using System.Net;

    public class ApiTokenMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiTokenMiddleware> _logger;

        public ApiTokenMiddleware(
            RequestDelegate next,
            ILogger<ApiTokenMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(
            HttpContext context,
            IConfiguration configuration)
        {
            // =====================================================
            // 1. Validate X-Environment
            // =====================================================

            var environment =
                context.Request.Headers["X-Environment"]
                    .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(environment))
            {
                context.Response.StatusCode =
                    StatusCodes.Status400BadRequest;

                await context.Response.WriteAsJsonAsync(new
                {
                    statusCode = 400,
                    message = "X-Environment header is required."
                });

                return;
            }

            // =====================================================
            // 2. Validate Authorization Header
            // =====================================================

            var authorizationHeader =
                context.Request.Headers["Authorization"]
                    .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(authorizationHeader))
            {
                context.Response.StatusCode =
                    StatusCodes.Status401Unauthorized;

                await context.Response.WriteAsJsonAsync(new
                {
                    statusCode = 401,
                    message = "Authorization header is required."
                });

                return;
            }

            // =====================================================
            // 3. Require Bearer scheme
            //
            // This is NOT JWT validation.
            // Bearer is only the header scheme.
            //
            // Authorization: Bearer <API_TOKEN>
            // =====================================================

            const string bearerPrefix = "Bearer ";

            if (!authorizationHeader.StartsWith(
                bearerPrefix,
                StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode =
                    StatusCodes.Status401Unauthorized;

                await context.Response.WriteAsJsonAsync(new
                {
                    statusCode = 401,
                    message =
                        "Authorization header must use the Bearer scheme."
                });

                return;
            }

            // =====================================================
            // 4. Extract opaque API token
            // =====================================================

            var apiToken = authorizationHeader
                .Substring(bearerPrefix.Length)
                .Trim();

            if (string.IsNullOrWhiteSpace(apiToken))
            {
                context.Response.StatusCode =
                    StatusCodes.Status401Unauthorized;

                await context.Response.WriteAsJsonAsync(new
                {
                    statusCode = 401,
                    message = "API token is required."
                });

                return;
            }

            // =====================================================
            // 5. Get configured API token
            // =====================================================

            //var validApiToken =
            //    configuration["ApiSecurity:Token"];

            //if (string.IsNullOrWhiteSpace(validApiToken))
            //{
            //    _logger.LogError(
            //        "API security token is not configured."
            //    );

            //    context.Response.StatusCode =
            //        StatusCodes.Status500InternalServerError;

            //    await context.Response.WriteAsJsonAsync(new
            //    {
            //        statusCode = 500,
            //        message =
            //            "API security configuration is invalid."
            //    });

            //    return;
            //}

            // =====================================================
            // 6. Compare API tokens
            // =====================================================



            // =====================================================
            // 7. Continue
            // =====================================================

            await _next(context);
        }
    }
}
