using Microsoft.AspNetCore.Http;

namespace Public.Infrastructure.Common
{
    public class HeaderHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HeaderHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? GetBearerToken()
        {
            var authorizationHeader = _httpContextAccessor
                .HttpContext?
                .Request
                .Headers
                .Authorization
                .ToString();

            if (string.IsNullOrWhiteSpace(authorizationHeader))
                return null;

            if (!authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return null;

            return authorizationHeader["Bearer ".Length..].Trim();
        }
                public string? GetSignature()
        {
            // Use the header name expected by LifePortal.
            // If your actual header is X-Signature, this already supports it.
            return GetHeaderValue("Signature")
                ?? GetHeaderValue("X-Signature")
                ?? GetHeaderValue("x-signature");
        }

        private string? GetHeaderValue(string headerName)
        {
            var headers = _httpContextAccessor.HttpContext?.Request.Headers;

            if (headers is null)
                return null;

            if (!headers.TryGetValue(headerName, out var value))
                return null;

            return value.ToString();
        }
    }
}