using Microsoft.AspNetCore.Http;
using Public.Application.Base;
using Public.Application.Interface;
using System.Net;
using System.Text;

namespace Public.Infrastructure.Service
{
    public class McpService : IMcpService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public McpService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<BaseResponse<bool>> HealthCheckAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                var client =
                    _httpClientFactory.CreateClient("McpServer");

                using var response =
                    await client.GetAsync(
                        "health",
                        cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    return BaseResponse<bool>.BadGateway(
                        $"MCP Server health check failed with status code {(int)response.StatusCode}.");
                }

                return BaseResponse<bool>.Ok(
                    true,
                    "MCP Server is running.");
            }
            catch (TaskCanceledException)
                when (!cancellationToken.IsCancellationRequested)
            {
                return new BaseResponse<bool>(
                    false,
                    "MCP Server health check timed out.",
                    HttpStatusCode.GatewayTimeout);
            }
            catch (HttpRequestException)
            {
                return BaseResponse<bool>.BadGateway(
                    "Unable to connect to MCP Server.");
            }
            catch (Exception)
            {
                return BaseResponse<bool>.InternalServerError(
                    "An unexpected error occurred while checking MCP Server.");
            }
        }


        public async Task<BaseResponse<string>> ConnectAsync(CancellationToken cancellationToken = default)
        {
            
            var httpContext =
                _httpContextAccessor.HttpContext;

            if (httpContext is null)
            {
                return BaseResponse<string>.InternalServerError(
                    "HTTP context is not available.");
            }

            var apiKey =
                httpContext.Request.Headers["X-API-KEY"]
                    .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return BaseResponse<string>.Unauthorized(
                    "X-API-KEY header is required.");
            }

            try
            {
                var client =
                    _httpClientFactory.CreateClient("McpServer");

                using var request =
                    new HttpRequestMessage(
                        HttpMethod.Post,
                        "mcp");

                /*
                 * Forward API key to MCP Server.
                 * MCP Server will validate it through AuthHub.
                 */
                request.Headers.TryAddWithoutValidation(
                    "X-API-KEY",
                    apiKey);

                /*
                 * MCP Streamable HTTP may return either
                 * application/json or text/event-stream.
                 */
                request.Headers.TryAddWithoutValidation(
                    "Accept",
                    "application/json, text/event-stream");

                /*
                 * Forward MCP-specific headers if supplied
                 * by the MCP client.
                 */
                CopyHeader(
                    httpContext.Request,
                    request,
                    "Mcp-Session-Id");

                CopyHeader(
                    httpContext.Request,
                    request,
                    "MCP-Protocol-Version");

                CopyHeader(
                    httpContext.Request,
                    request,
                    "Last-Event-ID");


                using var response =
                    await client.SendAsync(
                        request,
                        HttpCompletionOption.ResponseHeadersRead,
                        cancellationToken);

                var responseContent =
                    await response.Content.ReadAsStringAsync(
                        cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    /*
                     * Important:
                     * Preserve the MCP response body in Data
                     * for troubleshooting.
                     */
                    return new BaseResponse<string>(
                        responseContent,
                        $"MCP Server returned status code {(int)response.StatusCode}.",
                        HttpStatusCode.BadGateway);
                }

                return BaseResponse<string>.Ok(
                    responseContent,
                    "MCP request completed successfully.");
            }
            catch (TaskCanceledException)
                when (!cancellationToken.IsCancellationRequested)
            {
                return new BaseResponse<string>(
                    default,
                    "MCP Server request timed out.",
                    HttpStatusCode.GatewayTimeout);
            }
            catch (HttpRequestException)
            {
                return BaseResponse<string>.BadGateway(
                    "Unable to connect to MCP Server.");
            }
            catch (Exception)
            {
                return BaseResponse<string>.InternalServerError(
                    "An unexpected error occurred while communicating with MCP Server.");
            }
        }


        private static void CopyHeader(
            HttpRequest incomingRequest,
            HttpRequestMessage outgoingRequest,
            string headerName)
        {
            if (!incomingRequest.Headers.TryGetValue(
                    headerName,
                    out var values))
            {
                return;
            }

            outgoingRequest.Headers
                .TryAddWithoutValidation(
                    headerName,
                    values.ToArray());
        }
    }
}