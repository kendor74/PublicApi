using FluentValidation;
using Public.Application.DTO.Mcp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Public.Application.Validators
{
    public class McpRequestValidator
        : AbstractValidator<McpApiKeyHeader>
    {
        public McpRequestValidator()
        {
            RuleFor(x => x.ApiKey)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("X-API-KEY header is required.")
                .Must(apiKey =>
                    !string.IsNullOrWhiteSpace(apiKey) &&
                    apiKey.StartsWith(
                        "miclife_mcp_api_",
                        StringComparison.OrdinalIgnoreCase))
                .WithMessage(
                    "Invalid API key format.");
        }
    }
}
