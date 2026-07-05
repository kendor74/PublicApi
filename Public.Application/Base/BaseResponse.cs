using System.Net;
using System.Text.Json.Serialization;

namespace Public.Application.Base
{
    public class BaseResponse<T>
    {
        public bool Received { get; set; } = true;
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public HttpStatusCode HttpStatus => (HttpStatusCode)StatusCode;

        public BaseResponse()
        {
        }

        public BaseResponse(
            T? data,
            string message,
            HttpStatusCode statusCode,
            List<string>? errors = null)
        {
            Data = data;
            Message = message;
            StatusCode = (int)statusCode;
            Success = (int)statusCode >= 200 && (int)statusCode <= 299;
            Errors = errors ?? new List<string>();
        }

        public static BaseResponse<T> Ok(T data, string message = "Success")
        {
            return new BaseResponse<T>(data, message, HttpStatusCode.OK);
        }

        public static BaseResponse<T> Created(T data, string message = "Created successfully")
        {
            return new BaseResponse<T>(data, message, HttpStatusCode.Created);
        }

        public static BaseResponse<T> BadRequest(string message = "Bad request", List<string>? errors = null)
        {
            return new BaseResponse<T>(default, message, HttpStatusCode.BadRequest, errors);
        }

        public static BaseResponse<T> Unauthorized(string message = "Unauthorized")
        {
            return new BaseResponse<T>(default, message, HttpStatusCode.Unauthorized);
        }

        public static BaseResponse<T> Forbidden(string message = "Forbidden")
        {
            return new BaseResponse<T>(default, message, HttpStatusCode.Forbidden);
        }

        public static BaseResponse<T> NotFound(string message = "Not found")
        {
            return new BaseResponse<T>(default, message, HttpStatusCode.NotFound);
        }

        public static BaseResponse<T> Conflict(string message = "Conflict")
        {
            return new BaseResponse<T>(default, message, HttpStatusCode.Conflict);
        }

        public static BaseResponse<T> Unprocessable(string message = "Unprocessable entity", List<string>? errors = null)
        {
            return new BaseResponse<T>(default, message, HttpStatusCode.UnprocessableEntity, errors);
        }

        public static BaseResponse<T> BadGateway(string message = "Bad gateway")
        {
            return new BaseResponse<T>(default, message, HttpStatusCode.BadGateway);
        }

        public static BaseResponse<T> ServiceUnavailable(string message = "Service unavailable")
        {
            return new BaseResponse<T>(default, message, HttpStatusCode.ServiceUnavailable);
        }

        public static BaseResponse<T> InternalServerError(string message = "Internal server error")
        {
            return new BaseResponse<T>(default, message, HttpStatusCode.InternalServerError);
        }
    }
}