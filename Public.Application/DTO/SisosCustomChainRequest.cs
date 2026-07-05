using System.Text.Json;

namespace Public.Application.DTO
{
    public class SisosCustomChainRequest
    {
        public string Chain { get; set; } = string.Empty;

        public JsonElement Payload { get; set; }
    }
}