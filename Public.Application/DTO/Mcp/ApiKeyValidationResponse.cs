using System;
using System.Collections.Generic;
using System.Text;

namespace Public.Application.DTO.Mcp
{
    public class ApiKeyValidationResponse
    {
        public Guid ApiKeyId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string ClientType { get; set; } = string.Empty;

        public string Environment { get; set; } = string.Empty;

        public DateTime? ExpiresAt { get; set; }

        public IEnumerable<string> Permissions { get; set; }
            = Enumerable.Empty<string>();
    }
}
