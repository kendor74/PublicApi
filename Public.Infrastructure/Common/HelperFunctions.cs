using Public.Application.Base;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Public.Infrastructure.Common
{
    public class HelperFunctions
    {
        public JsonElement ConvertToJsonElement(string? responseText)
        {
            if (string.IsNullOrWhiteSpace(responseText))
            {
                return JsonSerializer.SerializeToElement<object?>(null);
            }

            try
            {
                return JsonElement.Parse(responseText);
            }
            catch (JsonException)
            {
                // Some downstream errors might be returned as plain text.
                return JsonSerializer.SerializeToElement(new
                {
                    message = responseText
                });
            }
        }

        public bool TryExtractSisosResponse(JsonElement responseBody, out JsonElement sisosResponse)
        {
            sisosResponse = default;

            // =====================================================
            // Expected structure:
            //
            // {
            //     "data": {
            //         "status": true,
            //         "payload": {
            //             ...
            //             "outData": {
            //                 ...
            //                 "data": { ... }
            //             }
            //         }
            //     }
            // }
            // =====================================================

            if (responseBody.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            // response.data
            if (!responseBody.TryGetProperty(
                    "data",
                    out var internalData) ||
                internalData.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            // response.data.payload
            if (!internalData.TryGetProperty(
                    "payload",
                    out var payload) ||
                payload.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            // response.data.payload.outData
            if (!payload.TryGetProperty(
                    "outData",
                    out var outData))
            {
                return false;
            }

            if (outData.ValueKind == JsonValueKind.Null ||
                outData.ValueKind == JsonValueKind.Undefined)
            {
                return false;
            }

            // =====================================================
            // Preferred structure:
            //
            // outData: {
            //     "data": { ... }
            // }
            //
            // Return only data to vendor.
            // =====================================================

            if (outData.ValueKind == JsonValueKind.Object &&
                outData.TryGetProperty(
                    "data",
                    out var data))
            {
                sisosResponse = data.Clone();

                return true;
            }

            // =====================================================
            // Fallback:
            //
            // Some chains may directly return:
            //
            // outData: {
            //     "policyId": 123,
            //     "amount": 500
            // }
            //
            // So return outData itself.
            // =====================================================

            sisosResponse = outData.Clone();

            return true;
        }

        public bool TryExtractSisosError(
            JsonElement responseBody,
            out string errorMessage,
            out JsonElement errorPayload)
        {
            errorMessage = string.Empty;
            errorPayload = default;

            if (responseBody.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            // =====================================================
            // Case 1:
            // Direct SISOS response
            //
            // {
            //     "ok": false,
            //     "msg": "Invalid token"
            // }
            // =====================================================

            if (TryExtractErrorFromPayload(
                responseBody,
                out errorMessage,
                out errorPayload))
            {
                return true;
            }

            // =====================================================
            // Case 2:
            //
            // {
            //     "data": {
            //         "status": false,
            //         "message": "...",
            //         "payload": {
            //             "ok": false,
            //             "msg": "Invalid token"
            //         }
            //     }
            // }
            // =====================================================

            if (!responseBody.TryGetProperty(
                    "data",
                    out var internalData) ||
                internalData.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            // =====================================================
            // First inspect data.payload.
            //
            // This is the most important case for your
            // internal SISOS API.
            // =====================================================

            if (internalData.TryGetProperty(
                    "payload",
                    out var payload) &&
                payload.ValueKind == JsonValueKind.Object)
            {
                if (TryExtractErrorFromPayload(
                    payload,
                    out errorMessage,
                    out errorPayload))
                {
                    return true;
                }
            }

            // =====================================================
            // Case 3:
            // Internal FetchResult itself reports failure.
            //
            // {
            //     "data": {
            //         "status": false,
            //         "message": "SISOS returned HTTP 400",
            //         "payload": {...}
            //     }
            // }
            // =====================================================

            if (!internalData.TryGetProperty(
                    "status",
                    out var statusElement) ||
                statusElement.ValueKind != JsonValueKind.False)
            {
                return false;
            }

            // =====================================================
            // Preserve payload if available.
            // Otherwise preserve the complete internal result.
            // =====================================================

            if (internalData.TryGetProperty(
                    "payload",
                    out var failedPayload) &&
                failedPayload.ValueKind != JsonValueKind.Null &&
                failedPayload.ValueKind != JsonValueKind.Undefined)
            {
                errorPayload = failedPayload.Clone();
            }
            else
            {
                errorPayload = internalData.Clone();
            }

            // =====================================================
            // Prefer payload.msg.
            //
            // This ensures:
            //
            // {
            //     "ok": false,
            //     "msg": "Invalid token"
            // }
            //
            // returns exactly:
            //
            // Invalid token
            // =====================================================

            if (failedPayload.ValueKind == JsonValueKind.Object &&
                failedPayload.TryGetProperty(
                    "msg",
                    out var payloadMessage))
            {
                if (payloadMessage.ValueKind == JsonValueKind.String)
                {
                    var message =
                        payloadMessage.GetString();

                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        errorMessage = message;
                        return true;
                    }
                }
                else if (
                    payloadMessage.ValueKind != JsonValueKind.Null &&
                    payloadMessage.ValueKind != JsonValueKind.Undefined)
                {
                    errorMessage =
                        payloadMessage.GetRawText();

                    return true;
                }
            }

            // =====================================================
            // Fall back to FetchResult.message
            // =====================================================

            if (internalData.TryGetProperty(
                    "message",
                    out var internalMessage) &&
                internalMessage.ValueKind == JsonValueKind.String)
            {
                var message =
                    internalMessage.GetString();

                if (!string.IsNullOrWhiteSpace(message))
                {
                    errorMessage = message;
                    return true;
                }
            }

            errorMessage =
                "SISOS request failed.";

            return true;
        }



        public string? ExtractResponseMessage(
    string bodyText)
        {
            if (string.IsNullOrWhiteSpace(bodyText))
                return null;

            try
            {
                var responseBody =
                    JsonElement.Parse(bodyText);

                if (responseBody.ValueKind !=
                    JsonValueKind.Object)
                {
                    return null;
                }

                if (responseBody.TryGetProperty(
                        "message",
                        out var messageElement) &&
                    messageElement.ValueKind ==
                        JsonValueKind.String)
                {
                    return messageElement.GetString();
                }

                return null;
            }
            catch (JsonException)
            {
                return null;
            }
        }
        public string? ExtractProviderCode(string bodyText)
        {
            if (string.IsNullOrWhiteSpace(bodyText))
                return null;

            try
            {
                var responseBody =
                    JsonElement.Parse(bodyText);

                if (responseBody.ValueKind != JsonValueKind.Object)
                    return null;

                if (!responseBody.TryGetProperty(
                        "data",
                        out var data))
                {
                    return null;
                }

                if (data.ValueKind != JsonValueKind.Object)
                    return null;

                if (!data.TryGetProperty(
                        "code",
                        out var code))
                {
                    return null;
                }

                return code.GetString();
            }
            catch (JsonException)
            {
                return null;
            }
        }
        
        
        public BaseResponse<object?> MapOtpResponse(string code)
        {
            return code switch
            {
                "4901" => new BaseResponse<object?>(
                    null,
                    "OTP sent successfully.",
                    HttpStatusCode.OK),

                "4903" => BaseResponse<object?>.BadGateway(
                    "SMS provider authentication failed."),

                "4904" => BaseResponse<object?>.BadGateway(
                    "SMS sender is not configured correctly."),

                "4905" => BaseResponse<object?>.BadRequest(
                    "Invalid mobile number."),

                "4906" => BaseResponse<object?>.ServiceUnavailable(
                    "Insufficient SMS credit."),

                "4907" => BaseResponse<object?>.ServiceUnavailable(
                    "SMS service is temporarily unavailable."),

                "4908" => BaseResponse<object?>.BadRequest(
                    "Invalid OTP."),

                "4909" => BaseResponse<object?>.BadGateway(
                    "OTP template is not configured correctly."),

                "4912" => BaseResponse<object?>.BadGateway(
                    "SMS environment is not configured correctly."),

                _ => BaseResponse<object?>.BadGateway(
                    "Unable to send OTP.")
            };
        }


        public BaseResponse<object?> MapSmsResponse(string code)
        {
            return code switch
            {
                "1901" => new BaseResponse<object?>(
                    null,
                    "SMS sent successfully.",
                    HttpStatusCode.OK),

                "1902" => BaseResponse<object?>.BadRequest(
                    "Invalid SMS request."),

                "1903" => BaseResponse<object?>.BadGateway(
                    "SMS provider authentication failed."),

                "1904" => BaseResponse<object?>.BadGateway(
                    "SMS sender is not configured correctly."),

                "1905" => BaseResponse<object?>.BadRequest(
                    "Invalid mobile number."),

                "1906" => BaseResponse<object?>.ServiceUnavailable(
                    "Insufficient SMS credit."),

                "1907" => BaseResponse<object?>.ServiceUnavailable(
                    "SMS service is temporarily unavailable."),

                "1908" => BaseResponse<object?>.BadRequest(
                    "Invalid SMS scheduling format."),

                "1909" => BaseResponse<object?>.BadRequest(
                    "Invalid SMS message."),

                "1910" => BaseResponse<object?>.BadRequest(
                    "Invalid SMS language."),

                "1911" => BaseResponse<object?>.BadRequest(
                    "SMS message is too long."),

                "1912" => BaseResponse<object?>.BadGateway(
                    "SMS environment is not configured correctly."),

                _ => BaseResponse<object?>.BadGateway(
                    "Unable to send SMS.")
            };
        }


        private bool TryExtractErrorFromPayload(
    JsonElement payload,
    out string errorMessage,
    out JsonElement errorPayload)
        {
            errorMessage = string.Empty;
            errorPayload = default;

            if (payload.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            // =====================================================
            // We only consider this a SISOS logical error when:
            //
            // "ok": false
            // =====================================================

            if (!payload.TryGetProperty(
                    "ok",
                    out var okElement) ||
                okElement.ValueKind != JsonValueKind.False)
            {
                return false;
            }

            // Preserve exact SISOS payload
            errorPayload =
                payload.Clone();

            // =====================================================
            // Extract msg exactly as SISOS returned it
            // =====================================================

            if (payload.TryGetProperty(
                    "msg",
                    out var msgElement))
            {
                if (msgElement.ValueKind == JsonValueKind.String)
                {
                    var message =
                        msgElement.GetString();

                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        errorMessage = message;
                        return true;
                    }
                }

                // In case SISOS unexpectedly sends something
                // other than a string in msg.
                if (msgElement.ValueKind != JsonValueKind.Null &&
                    msgElement.ValueKind != JsonValueKind.Undefined)
                {
                    errorMessage =
                        msgElement.GetRawText();

                    return true;
                }
            }

            errorMessage =
                "SISOS request failed.";

            return true;
        }
    }
}
