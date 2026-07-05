using System.Text.Json;

namespace Public.Application.DTO
{
    public class SisosCmdRequest
    {
        public string Command { get; set; } = string.Empty;
        public JsonElement Context { get; set; }
    }
}