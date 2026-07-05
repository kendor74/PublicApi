using Microsoft.AspNetCore.Mvc;
using Public.Application.Base;

namespace Public.Api.Base
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppControllerBase : ControllerBase
    {
        protected IActionResult CustomResult<T>(BaseResponse<T> response)
        {
            // Gateway rule:
            // HTTP status code is always 200 OK.
            // The actual function/business status is inside response.StatusCode.
            return Ok(response);
        }

        protected IActionResult ServerReceived<T>(BaseResponse<T> response)
        {
            return Ok(response);
        }
    }
}