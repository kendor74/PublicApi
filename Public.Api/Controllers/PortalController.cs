using Microsoft.AspNetCore.Mvc;
using Public.Api.Base;
using Public.Application.DTO;
using Public.Application.Interface;

namespace Public.Api.Controllers
{
    //[ApiVersion(1.0)]
    //[Route("api/v{version:apiVersion}/[controller]")]
    [Route("api/[controller]")]
    public class PortalController : AppControllerBase
    {
        private readonly IPortalServices _portalServices;

        public PortalController(IPortalServices portalServices)
        {
            _portalServices = portalServices;
        }

        [HttpGet("get-portal-response")]
        public async Task<IActionResult> GetPortal(CancellationToken cancellationToken)
        {
            var result = await _portalServices.GetPortalResponseAsync(cancellationToken);
            return CustomResult(result);
        }

        [HttpPost("sisos-login")]
        public async Task<IActionResult> SisosLogin(
            [FromBody] SisosLoginRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _portalServices.SisosLoginAsync(request, cancellationToken);
            return CustomResult(result);
        }
        //[HttpPost("sisos-register")]
        //public async Task<IActionResult> SisosRegister([FromBody] SisosLoginRequest request, CancellationToken cancellationToken)
        //{
        //    var result = await _portalServices.SisosLoginAsync(request, cancellationToken);
        //    return CustomResult(result);
        //}

        //[HttpPost("sisos-rest-password")]
        //public async Task<IActionResult> SisosRestPassword([FromBody] SisosCustomChainRequest request, CancellationToken cancellationToken)
        //{
        //    var result = await _portalServices.SisosCustomChainAsync(
        //        request,
        //        cancellationToken
        //    );

        //    return CustomResult(result);
        //}

        [HttpPost("sisos-cmd")]
        public async Task<IActionResult> SisosCmd(
            [FromBody] SisosCmdRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _portalServices.SisosCmdAsync(
                request,
                cancellationToken
            );

            return CustomResult(result);
        }
        [HttpPost("sisos-custom-chain")]
        public async Task<IActionResult> SisosCustomChain([FromBody] SisosCustomChainRequest request, CancellationToken cancellationToken)
        {
            var result = await _portalServices.SisosCustomChainAsync(
                request,
                cancellationToken
            );

            return CustomResult(result);
        }

        //[HttpPost("sisos-int")]
        //public async Task<IActionResult> SisosInt([FromBody] SisosCustomChainRequest request, CancellationToken cancellationToken)
        //{
        //    var result = await _portalServices.SisosCustomChainAsync(
        //        request,
        //        cancellationToken
        //    );

        //    return CustomResult(result);
        //}
        
    }
}