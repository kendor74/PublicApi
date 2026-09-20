using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Public.Application.Base;
using Public.Application.DTO;
using Public.Application.Interface;
using Public.Infrastructure.Common;
using Public.Infrastructure.Configuration;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Public.Infrastructure.Services
{
    public class PortalService : IPortalServices
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IValidator<SisosLoginRequest> _sisosLoginValidator;
        private readonly IValidator<SisosCmdRequest> _sisosCmdValidator;
        private readonly IValidator<SisosCustomChainRequest> _sisosCustomChainValidator;
        private readonly IValidator<SmsOTPRequest> _smsOtpValidator;
        private readonly IValidator<SmsMessageRequest> _smsMessageValidator;
        private readonly ILogger<PortalService> _logger;
        private readonly HelperFunctions _helperFunctions;
        private readonly InternalApis _api;
        private readonly HeaderHelper _headerHelper;

        public PortalService(IHttpClientFactory httpClientFactory, IValidator<SisosLoginRequest> sisosLoginValidator, IValidator<SisosCmdRequest> sisosCmdValidator,
         ILogger<PortalService> logger, IValidator<SisosCustomChainRequest> sisosCustomChainValidator, IValidator<SmsOTPRequest> smsOtpValidator, IValidator<SmsMessageRequest> smsMessageValidator,
         IOptions<InternalApis> api, HeaderHelper headerHelper, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _sisosLoginValidator = sisosLoginValidator;
            _sisosCmdValidator = sisosCmdValidator;
            _sisosCustomChainValidator = sisosCustomChainValidator;
            _smsMessageValidator = smsMessageValidator;
            _smsOtpValidator = smsOtpValidator;
            _logger = logger;
            _api = api.Value;
            _headerHelper = headerHelper;
            _httpContextAccessor = httpContextAccessor;
            _helperFunctions = new HelperFunctions();
        }

        public async Task<BaseResponse<string>> GetPortalResponseAsync(CancellationToken cancellationToken)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("LifePortal");

                using var response = await client.GetAsync(
                    "",
                    cancellationToken
                );

                var body = await response.Content.ReadAsStringAsync(cancellationToken);

                _logger.LogInformation(
                    "LifePortal root endpoint returned status code {StatusCode}",
                    (int)response.StatusCode
                );

                if (!response.IsSuccessStatusCode)
                {
                    return new BaseResponse<string>(
                        data: body,
                        message: $"LifePortal returned error with status code {(int)response.StatusCode}",
                        statusCode: response.StatusCode
                    );
                }

                return new BaseResponse<string>(
                    data: body,
                    message: "LifePortal response returned successfully",
                    statusCode: response.StatusCode
                );
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("LifePortal request timed out");

                return BaseResponse<string>.ServiceUnavailable(
                    "LifePortal request timed out"
                );
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to call LifePortal");

                return BaseResponse<string>.BadGateway(
                    "Failed to call LifePortal"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while calling LifePortal");

                return BaseResponse<string>.InternalServerError(
                    "Unexpected error while calling LifePortal"
                );
            }
        }

        public async Task<BaseResponse<JsonElement>> SisosLoginAsync(SisosLoginRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var validationResult = await _sisosLoginValidator.ValidateAsync(
                    request,
                    cancellationToken
                );

                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors
                        .Select(error => error.ErrorMessage)
                        .ToList();

                    return new BaseResponse<JsonElement>(
                        data: JsonSerializer.SerializeToElement(new
                        {
                            errors
                        }),
                        message: "Validation failed",
                        statusCode: HttpStatusCode.BadRequest
                    );
                }

                var client = _httpClientFactory.CreateClient("LifePortal");

                using var response = await client.PostAsJsonAsync(
                    "api/Sisos/sisos-auth",
                    request,
                    cancellationToken
                );

                var bodyText = await response.Content.ReadAsStringAsync(
                    cancellationToken
                );

                JsonElement responseBody;

                try
                {
                    responseBody = JsonElement.Parse(bodyText);
                }
                catch (JsonException)
                {
                    // Handles cases where the external API returns plain text.
                    responseBody = JsonSerializer.SerializeToElement(new
                    {
                        message = bodyText
                    });
                }

                _logger.LogInformation(
                    "Sisos login API responded with status code {StatusCode}",
                    (int)response.StatusCode
                );

                if (!response.IsSuccessStatusCode)
                {
                    return new BaseResponse<JsonElement>(
                        data: responseBody,
                        message:
                            $"Sisos login failed with status code {(int)response.StatusCode}",
                        statusCode: response.StatusCode
                    );
                }

                return new BaseResponse<JsonElement>(
                    data: responseBody,
                    message: "Sisos login completed successfully",
                    statusCode: response.StatusCode
                );
            }
            catch (OperationCanceledException)
                when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Sisos login request timed out");

                return new BaseResponse<JsonElement>(
                    data: JsonSerializer.SerializeToElement(new
                    {
                        error = "Sisos login request timed out"
                    }),
                    message: "Sisos login request timed out",
                    statusCode: HttpStatusCode.ServiceUnavailable
                );
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to call Sisos login API");

                return new BaseResponse<JsonElement>(
                    data: JsonSerializer.SerializeToElement(new
                    {
                        error = "Failed to call Sisos login API"
                    }),
                    message: "Failed to call Sisos login API",
                    statusCode: HttpStatusCode.BadGateway
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error while calling Sisos login API"
                );

                return new BaseResponse<JsonElement>(
                    data: JsonSerializer.SerializeToElement(new
                    {
                        error = "Unexpected error while calling Sisos login API"
                    }),
                    message: "Unexpected error while calling Sisos login API",
                    statusCode: HttpStatusCode.InternalServerError
                );
            }
        }


        //public async Task<BaseResponse<JsonElement>> SisosCmdAsync(SisosCmdRequest request, CancellationToken cancellationToken)
        //{
        //    var validationResult = await _sisosCmdValidator.ValidateAsync(
        //        request,
        //        cancellationToken
        //    );

        //    if (!validationResult.IsValid)
        //    {
        //        var errors = validationResult.Errors
        //            .Select(error => error.ErrorMessage)
        //            .ToList();

        //        return new BaseResponse<JsonElement>(
        //            data: JsonSerializer.SerializeToElement(new
        //            {
        //                errors
        //            }),
        //            message: "Validation failed",
        //            statusCode: HttpStatusCode.BadRequest
        //        );
        //    }

        //    var bearerToken = _headerHelper.GetBearerToken();

        //    if (string.IsNullOrWhiteSpace(bearerToken))
        //    {
        //        return new BaseResponse<JsonElement>(
        //            data: JsonSerializer.SerializeToElement(new
        //            {
        //                error = "Bearer token is required."
        //            }),
        //            message: "Bearer token is required.",
        //            statusCode: HttpStatusCode.Unauthorized
        //        );
        //    }

        //    try
        //    {
        //        var client = _httpClientFactory.CreateClient("LifePortal");

        //        var requestBody = new
        //        {
        //            command = request.Command,
        //            context = request.Context
        //        };

        //        using var httpRequest = new HttpRequestMessage(
        //            HttpMethod.Post,
        //            "api/Sisos/Cmd"
        //        );

        //        httpRequest.Headers.Authorization =
        //            new AuthenticationHeaderValue(
        //                "Bearer",
        //                bearerToken
        //            );

        //        httpRequest.Content = JsonContent.Create(requestBody);

        //        using var response = await client.SendAsync(
        //            httpRequest,
        //            cancellationToken
        //        );

        //        var responseText = await response.Content.ReadAsStringAsync(
        //            cancellationToken
        //        );

        //        var responseBody = _helperFunctions.ConvertToJsonElement(responseText);

        //        _logger.LogInformation(
        //            "Sisos CMD returned status code {StatusCode}",
        //            (int)response.StatusCode
        //        );

        //        if (!response.IsSuccessStatusCode)
        //        {
        //            _logger.LogWarning(
        //                "Sisos CMD failed with status code {StatusCode}. Response: {Response}",
        //                (int)response.StatusCode,
        //                responseText
        //            );
        //        }

        //        return new BaseResponse<JsonElement>(
        //            data: responseBody,
        //            message: response.IsSuccessStatusCode
        //                ? "Sisos command completed successfully"
        //                : $"Sisos command failed with status code {(int)response.StatusCode}",
        //            statusCode: response.StatusCode
        //        );
        //    }
        //    catch (OperationCanceledException)
        //        when (!cancellationToken.IsCancellationRequested)
        //    {
        //        _logger.LogWarning(
        //            "Sisos CMD request timed out"
        //        );

        //        return new BaseResponse<JsonElement>(
        //            data: JsonSerializer.SerializeToElement(new
        //            {
        //                error = "Sisos CMD request timed out"
        //            }),
        //            message: "Sisos CMD request timed out",
        //            statusCode: HttpStatusCode.ServiceUnavailable
        //        );
        //    }
        //    catch (HttpRequestException ex)
        //    {
        //        _logger.LogError(
        //            ex,
        //            "Failed to call Sisos CMD API"
        //        );

        //        return new BaseResponse<JsonElement>(
        //            data: JsonSerializer.SerializeToElement(new
        //            {
        //                error = "Failed to call Sisos CMD API"
        //            }),
        //            message: "Failed to call Sisos CMD API",
        //            statusCode: HttpStatusCode.BadGateway
        //        );
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(
        //            ex,
        //            "Unexpected error while calling Sisos CMD API"
        //        );

        //        return new BaseResponse<JsonElement>(
        //            data: JsonSerializer.SerializeToElement(new
        //            {
        //                error = "Unexpected error while calling Sisos CMD API"
        //            }),
        //            message: "Unexpected error while calling Sisos CMD API",
        //            statusCode: HttpStatusCode.InternalServerError
        //        );
        //    }
        //}

        public async Task<BaseResponse<JsonElement>> SisosCustomChainAsync(SisosCustomChainRequest request, CancellationToken cancellationToken)
        {
            // =====================================================
            // Validate Request
            // =====================================================

            var validationResult =
                await _sisosCustomChainValidator.ValidateAsync(
                    request,
                    cancellationToken
                );

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(error => error.ErrorMessage)
                    .ToList();

                return new BaseResponse<JsonElement>(
                    data: JsonSerializer.SerializeToElement(new
                    {
                        errors
                    }),
                    message: "Validation failed",
                    statusCode: HttpStatusCode.BadRequest
                );
            }

            // =====================================================
            // Get API Token
            // =====================================================

            var bearerToken =
                _headerHelper.GetBearerToken();

            if (string.IsNullOrWhiteSpace(bearerToken))
            {
                return new BaseResponse<JsonElement>(
                    data: JsonSerializer.SerializeToElement(new
                    {
                        error = "Bearer token is required."
                    }),
                    message: "Bearer token is required.",
                    statusCode: HttpStatusCode.Unauthorized
                );
            }

            // =====================================================
            // Get HttpContext
            // =====================================================

            var httpContext =
                _httpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                return new BaseResponse<JsonElement>(
                    data: JsonSerializer.SerializeToElement(new
                    {
                        error = "HTTP context is not available."
                    }),
                    message: "HTTP context is not available.",
                    statusCode:
                        HttpStatusCode.InternalServerError
                );
            }

            // =====================================================
            // Get Environment
            // =====================================================

            var environment =
                httpContext.Request
                    .Headers["X-Environment"]
                    .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(environment))
            {
                return new BaseResponse<JsonElement>(
                    data: JsonSerializer.SerializeToElement(new
                    {
                        error =
                            "X-Environment header is required."
                    }),
                    message:
                        "X-Environment header is required.",
                    statusCode:
                        HttpStatusCode.BadRequest
                );
            }

            try
            {
                var client =
                    _httpClientFactory.CreateClient(
                        "LifePortal"
                    );


                var requestBody = new
                {
                    command = "ExeChain",

                    data = new
                    {
                        chain = request.Chain,
                        context = request.Context
                    }
                };

                // =====================================================
                // Create Internal Request
                // =====================================================

                using var httpRequest =
                    new HttpRequestMessage(
                        HttpMethod.Post,
                        "api/Sisos/sisos-int-api"
                    );

                // =====================================================
                // Authorization API Token
                // =====================================================

                httpRequest.Headers.Authorization =
                    new AuthenticationHeaderValue(
                        "Bearer",
                        bearerToken
                    );

                // =====================================================
                // Environment
                // =====================================================

                httpRequest.Headers.TryAddWithoutValidation(
                    "X-Environment",
                    environment
                );

                // =====================================================
                // Body
                // =====================================================

                httpRequest.Content =
                    JsonContent.Create(requestBody);

                // =====================================================
                // Execute Internal API
                // =====================================================

                using var response =
                    await client.SendAsync(
                        httpRequest,
                        cancellationToken
                    );

                var responseText =
                    await response.Content
                        .ReadAsStringAsync(
                            cancellationToken
                        );

                var responseBody =
                    _helperFunctions.ConvertToJsonElement(
                        responseText
                    );

                if (_helperFunctions.TryExtractSisosError(
                    responseBody,
                    out var sisosErrorMessage,
                    out var sisosErrorPayload))
                {
                    _logger.LogWarning(
                        "SISOS returned an error. Message: {Message}. Payload: {Payload}",
                        sisosErrorMessage,
                        sisosErrorPayload.GetRawText()
                    );

                    HttpStatusCode errorStatusCode;

                    if (!response.IsSuccessStatusCode)
                    {
                        errorStatusCode =
                            response.StatusCode;
                    }
                    else
                    {

                        errorStatusCode =
                            HttpStatusCode.BadRequest;

                        if (sisosErrorMessage.Contains(
                            "token",
                            StringComparison.OrdinalIgnoreCase))
                        {
                            errorStatusCode =
                                HttpStatusCode.Unauthorized;
                        }
                    }

                    return new BaseResponse<JsonElement>(
                        data: sisosErrorPayload,
                        message: sisosErrorMessage,
                        statusCode: errorStatusCode
                    );
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Sisos Custom Chain failed with HTTP status code {StatusCode}. Response: {Response}",
                        (int)response.StatusCode,
                        responseText
                    );

                    return new BaseResponse<JsonElement>(
                        data: responseBody,
                        message:
                            $"Sisos custom chain failed with status code {(int)response.StatusCode}",
                        statusCode:
                            response.StatusCode
                    );
                }

                if (!_helperFunctions.TryExtractSisosResponse(
                    responseBody,
                    out var sisosResponse))
                {
                    _logger.LogWarning(
                        "SISOS response did not contain the expected response structure. Response: {Response}",
                        responseText
                    );

                    return new BaseResponse<JsonElement>(
                        data: JsonSerializer.SerializeToElement(new
                        {
                            error =
                                "The SISOS response has an unexpected structure."
                        }),
                        message:
                            "SISOS response was invalid.",
                        statusCode:
                            HttpStatusCode.BadGateway
                    );
                }

                // =====================================================
                // Success
                // =====================================================

                return new BaseResponse<JsonElement>(
                    data: sisosResponse,
                    message:
                        "Sisos custom chain completed successfully",
                    statusCode:
                        HttpStatusCode.OK
                );
            }
            catch (OperationCanceledException)
                when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(
                    "Sisos custom chain request timed out"
                );

                return new BaseResponse<JsonElement>(
                    data: JsonSerializer.SerializeToElement(new
                    {
                        error =
                            "Sisos custom chain request timed out"
                    }),
                    message:
                        "Sisos custom chain request timed out",
                    statusCode:
                        HttpStatusCode.ServiceUnavailable
                );
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to call Sisos Custom Chain API"
                );

                return new BaseResponse<JsonElement>(
                    data: JsonSerializer.SerializeToElement(new
                    {
                        error =
                            "Failed to call Sisos Custom Chain API"
                    }),
                    message:
                        "Failed to call Sisos Custom Chain API",
                    statusCode:
                        HttpStatusCode.BadGateway
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error while calling Sisos Custom Chain API"
                );

                return new BaseResponse<JsonElement>(
                    data: JsonSerializer.SerializeToElement(new
                    {
                        error =
                            "Unexpected error while calling Sisos Custom Chain API"
                    }),
                    message:
                        "Unexpected error while calling Sisos Custom Chain API",
                    statusCode:
                        HttpStatusCode.InternalServerError
                );
            }
        }

        public async Task<BaseResponse<object?>> SmsSendOTPAsync(SmsOTPRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var validationResult =
                    await _smsOtpValidator.ValidateAsync(
                        request,
                        cancellationToken);

                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors
                        .Select(x => x.ErrorMessage)
                        .ToList();

                    return BaseResponse<object?>.BadRequest(
                        "Validation failed.",
                        errors);
                }

                var client =
                    _httpClientFactory.CreateClient("LifePortal");

                using var response =
                    await client.PostAsJsonAsync(
                        "api/SMS/otp",
                        request,
                        cancellationToken);

                var bodyText =
                    await response.Content.ReadAsStringAsync(
                        cancellationToken);

                var message =
                    _helperFunctions.ExtractResponseMessage(
                        bodyText);

                _logger.LogInformation(
                    "SMS OTP API responded with HTTP {StatusCode}. Message: {Message}",
                    (int)response.StatusCode,
                    message);

                if (string.IsNullOrWhiteSpace(message))
                {
                    message = response.IsSuccessStatusCode
                        ? "OTP request completed successfully."
                        : "OTP service request failed.";
                }

                return new BaseResponse<object?>(
                    data: null,
                    message: message,
                    statusCode: response.StatusCode);
            }
            catch (OperationCanceledException)
                when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(
                    "OTP service request timed out.");

                return BaseResponse<object?>.ServiceUnavailable(
                    "OTP service request timed out.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to call OTP service.");

                return BaseResponse<object?>.BadGateway(
                    "OTP service is currently unavailable.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error while sending OTP.");

                return BaseResponse<object?>.InternalServerError(
                    "An unexpected error occurred while sending OTP.");
            }
        }

        public async Task<BaseResponse<object?>> SmsSendMessageAsync(SmsMessageRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var validationResult =
                    await _smsMessageValidator.ValidateAsync(
                        request,
                        cancellationToken);

                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors
                        .Select(x => x.ErrorMessage)
                        .ToList();

                    return BaseResponse<object?>.BadRequest(
                        "Validation failed.",
                        errors);
                }

                var client =
                    _httpClientFactory.CreateClient("LifePortal");

                using var response =
                    await client.PostAsJsonAsync(
                        "api/Portal/SMS/message",
                        request,
                        cancellationToken);

                var bodyText =
                    await response.Content.ReadAsStringAsync(
                        cancellationToken);

                var message =
                    _helperFunctions.ExtractResponseMessage(
                        bodyText);

                _logger.LogInformation(
                    "SMS API responded with HTTP {StatusCode}. Message: {Message}",
                    (int)response.StatusCode,
                    message);

                if (string.IsNullOrWhiteSpace(message))
                {
                    message = response.IsSuccessStatusCode
                        ? "SMS request completed successfully."
                        : "SMS service request failed.";
                }

                return new BaseResponse<object?>(
                    data: null,
                    message: message,
                    statusCode: response.StatusCode);
            }
            catch (OperationCanceledException)
                when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(
                    "SMS service request timed out.");

                return BaseResponse<object?>.ServiceUnavailable(
                    "SMS service request timed out.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to call SMS service.");

                return BaseResponse<object?>.BadGateway(
                    "SMS service is currently unavailable.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error while sending SMS.");

                return BaseResponse<object?>.InternalServerError(
                    "An unexpected error occurred while sending SMS.");
            }
        }

    }
}
