namespace Public.Api.Middlerware
{
    public class EnvironmentHeaderMiddleware
    {
        private readonly RequestDelegate _next;

        public EnvironmentHeaderMiddleware(
            RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context)
        {
            var environment =
                context.Request.Headers["X-Environment"]
                    .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(environment))
            {
                context.Response.StatusCode =
                    StatusCodes.Status400BadRequest;

                context.Response.ContentType =
                    "application/json";

                await context.Response.WriteAsJsonAsync(new
                {
                    statusCode = 400,
                    message = "X-Environment header is required."
                });

                return;
            }

            await _next(context);
        }
    }
}
