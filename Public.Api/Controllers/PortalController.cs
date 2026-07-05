using Microsoft.AspNetCore.Mvc;
using Public.Api.Base;
using Public.Application.Base;
using Public.Application.DTO;
using Public.Application.Interface;

namespace Public.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PortalController : AppControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<PortalController> _logger;
        private readonly IPortalServices _portalServices;
        public PortalController(IHttpClientFactory httpClientFactory, ILogger<PortalController> logger, IPortalServices portalServices)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _portalServices = portalServices;
        }


        [HttpGet("get-portal-response")]
        public async Task<IActionResult> GetPortal(CancellationToken cancellationToken)
        {
            var client = _httpClientFactory.CreateClient("LifePortal");

            using var response = await client.GetAsync("/", cancellationToken);

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogInformation(
                "LifePortal responded with status code {StatusCode}",
                (int)response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode(
                    (int)response.StatusCode,
                    new Application.Base.BaseResponse<string>(
                        $"LifePortal returned error: {(int)response.StatusCode}"));
            }

            return CustomResult(
                new Application.Base.BaseResponse<string>(content));
        }

        [HttpPost("sisos-login")]
        public async Task<IActionResult> SisosLogin(SisosLoginRequest request ,CancellationToken cancellationToken)
        {
            var result = await _portalServices.SisosLoginAsync(request, cancellationToken);

            if (result.StatusCode < 200 || result.StatusCode >= 300)
                return StatusCode(result.StatusCode, new BaseResponse<string>(result.Body));

            return CustomResult(new BaseResponse<string>(result.Body));
        }


    }
}
