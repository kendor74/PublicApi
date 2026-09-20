using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Headers;

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
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext is null)
            {
                return null;
            }

            var authorizationHeader = httpContext.Request.Headers[
                HeaderNames.Authorization
            ].ToString();

            if (string.IsNullOrWhiteSpace(authorizationHeader))
            {
                return null;
            }

            if (!AuthenticationHeaderValue.TryParse(
                    authorizationHeader,
                    out var authenticationHeader))
            {
                return null;
            }

            if (!string.Equals(
                    authenticationHeader.Scheme,
                    "Bearer",
                    StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var token = authenticationHeader.Parameter?.Trim();

            return string.IsNullOrWhiteSpace(token)
                ? null
                : token;
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