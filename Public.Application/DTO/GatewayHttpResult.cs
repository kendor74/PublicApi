using System;
using System.Collections.Generic;
using System.Text;

namespace Public.Application.DTO
{
    public sealed class GatewayHttpResult
    {
        public required int StatusCode { get; init; }
        public required string Body { get; init; }
        public string? ContentType { get; init; }
    }
}
