using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Public.Application.Base
{
    public class BaseResponse<T>
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        public BaseResponse(T data, string message = "", HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            Data = data;
            Message = message;
            StatusCode = statusCode;
        }

        public static BaseResponse<T> Success(T data, string message = "Success") =>
            new(data, message, HttpStatusCode.OK);

        public static BaseResponse<T> Fail(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest) =>
            new(default!, message, statusCode);
    }
}
