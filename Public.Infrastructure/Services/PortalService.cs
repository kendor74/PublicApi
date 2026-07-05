using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Public.Application.DTO;
using Public.Application.Interface;
using Public.Infrastructure.Configuration;

namespace Public.Infrastructure.Services
{
    public class PortalService : IPortalServices
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PortalService> _logger;
        private readonly InternalApis _api;

        public PortalService(HttpClient httpClient, ILogger<PortalService> logger, IOptions<InternalApis> api)
        {
            _httpClient = httpClient;
            _logger = logger;
            _api = api.Value;
        }

        public Task<GatewayHttpResult> PortalHealthCheck()
        {
            throw new NotImplementedException();
        }

        public Task<GatewayHttpResult> SisosLoginAsync(SisosLoginRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
