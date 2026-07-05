using Microsoft.AspNetCore.Mvc;
using System.Net;
using Public.Application.Base;

namespace Public.Api.Base
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppControllerBase : ControllerBase
    {
        protected ObjectResult CustomResult<T>(BaseResponse<T> response)
        {
            return response.StatusCode switch
            {
                HttpStatusCode.OK => Ok(response),
                HttpStatusCode.Created => Created(string.Empty, response),
                HttpStatusCode.BadRequest => BadRequest(response),
                HttpStatusCode.Unauthorized => Unauthorized(response),
                HttpStatusCode.NotFound => NotFound(response),
                HttpStatusCode.UnprocessableEntity => UnprocessableEntity(response),
                _ => BadRequest(response)
            };
        }

        
    }
}
