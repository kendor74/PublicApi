using System;
using System.Collections.Generic;
using System.Text;

namespace Public.Application.DTO
{
    public class SmsOTPRequest
    {
        public string Mobile { get; set; } = string.Empty;
        public string OTP { get; set; } = string.Empty;
    }
}
