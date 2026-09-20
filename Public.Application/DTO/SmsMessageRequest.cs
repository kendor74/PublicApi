using System;
using System.Collections.Generic;
using System.Text;

namespace Public.Application.DTO
{
    public class SmsMessageRequest
    {
        public string Mobile { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
