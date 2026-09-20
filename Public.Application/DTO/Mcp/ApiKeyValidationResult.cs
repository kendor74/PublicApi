using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Public.Application.DTO.Mcp
{
    public class ApiKeyValidationResult
    {
        public bool IsValid { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public string Message { get; set; } = string.Empty;

        public ApiKeyValidationResponse? Data { get; set; }
    }
}
