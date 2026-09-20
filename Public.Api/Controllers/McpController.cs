using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Public.Api.Base;
using Public.Application.DTO.Mcp;
using Public.Application.Interface;
using System.Text.Json;

namespace Public.Api.Controllers
{
    public class McpController : AppControllerBase
    {
        private readonly IMcpService _mcpService;
        private readonly IValidator<McpApiKeyHeader> _apiKeyValidator;

        public McpController(
            IMcpService mcpService,
            IValidator<McpApiKeyHeader> apiKeyValidator)
        {
            _mcpService = mcpService;
            _apiKeyValidator = apiKeyValidator;
        }
        [HttpGet("health")]
        public async Task<IActionResult> HealthCheck(
            CancellationToken cancellationToken)
        {
            var result =
                await _mcpService.HealthCheckAsync(
                    cancellationToken);

            return CustomResult(result);
        }

        
    }
}