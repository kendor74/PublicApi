using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Public.Application.Base;
using Public.Application.DTO;
using Public.Application.Interface;
using Public.Infrastructure.Common;
using Public.Infrastructure.Configuration;

namespace Public.Infrastructure.Services
{
    public class PortalService : IPortalServices
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IValidator<SisosLoginRequest> _sisosLoginValidator;
        private readonly IValidator<SisosCmdRequest> _sisosCmdValidator;
        private readonly IValidator<SisosCustomChainRequest> _sisosCustomChainValidator;
        private readonly ILogger<PortalService> _logger;
        private readonly InternalApis _api;
        private readonly HeaderHelper _headerHelper;

        public PortalService(IHttpClientFactory httpClientFactory, IValidator<SisosLoginRequest> sisosLoginValidator, IValidator<SisosCmdRequest> sisosCmdValidator, ILogger<PortalService> logger,
         IOptions<InternalApis> api, HeaderHelper headerHelper)
        {
            _httpClientFactory = httpClientFactory;
            _sisosLoginValidator = sisosLoginValidator;
            _sisosCmdValidator = sisosCmdValidator;
            _logger = logger;
            _api = api.Value;
            _headerHelper = headerHelper;
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

        public async Task<BaseResponse<string>> SisosLoginAsync(SisosLoginRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("LifePortal");

                using var response = await client.PostAsJsonAsync(
                    "PortalApi/Sisos/login",
                    request,
                    cancellationToken
                );

                var body = await response.Content.ReadAsStringAsync(cancellationToken);

                _logger.LogInformation(
                    "Sisos login API responded with status code {StatusCode}",
                    (int)response.StatusCode
                );

                if (!response.IsSuccessStatusCode)
                {
                    return new BaseResponse<string>(
                        data: body,
                        message: $"Sisos login failed with status code {(int)response.StatusCode}",
                        statusCode: response.StatusCode
                    );
                }

                return new BaseResponse<string>(
                    data: body,
                    message: "Sisos login completed successfully",
                    statusCode: response.StatusCode
                );
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Sisos login request timed out");

                return BaseResponse<string>.ServiceUnavailable(
                    "Sisos login request timed out"
                );
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to call Sisos login API");

                return BaseResponse<string>.BadGateway(
                    "Failed to call Sisos login API"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while calling Sisos login API");

                return BaseResponse<string>.InternalServerError(
                    "Unexpected error while calling Sisos login API"
                );
            }
        }

        public async Task<BaseResponse<string>> SisosCmdAsync(
            SisosCmdRequest request,
            CancellationToken cancellationToken)
        {
            var validationResult = await _sisosCmdValidator.ValidateAsync(
                request,
                cancellationToken
            );

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(x => x.ErrorMessage)
                    .ToList();

                return BaseResponse<string>.BadRequest(
                    message: "Validation failed",
                    errors: errors
                );
            }

            var bearerToken = _headerHelper.GetBearerToken();

            if (string.IsNullOrWhiteSpace(bearerToken))
            {
                return BaseResponse<string>.Unauthorized(
                    "Bearer token is required."
                );
            }

            try
            {
                var client = _httpClientFactory.CreateClient("LifePortal");

                var body = new
                {
                    command = request.Command,
                    context = request.Context
                };

                using var httpRequest = new HttpRequestMessage(
                    HttpMethod.Post,
                    "PortalApi/Sisos/Cmd"
                );

                httpRequest.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue(
                        "Bearer",
                        bearerToken
                    );

                httpRequest.Content = JsonContent.Create(body);

                using var response = await client.SendAsync(
                    httpRequest,
                    cancellationToken
                );

                var responseBody = await response.Content.ReadAsStringAsync(
                    cancellationToken
                );

                _logger.LogInformation(
                    "Sisos CMD returned status code {StatusCode}",
                    (int)response.StatusCode
                );

                return new BaseResponse<string>(
                    data: responseBody,
                    message: response.IsSuccessStatusCode
                        ? "Sisos command completed successfully"
                        : $"Sisos command failed with status code {(int)response.StatusCode}",
                    statusCode: response.StatusCode
                );
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return BaseResponse<string>.ServiceUnavailable(
                    "Sisos CMD request timed out"
                );
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to call Sisos CMD API");

                return BaseResponse<string>.BadGateway(
                    "Failed to call Sisos CMD API"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while calling Sisos CMD API");

                return BaseResponse<string>.InternalServerError(
                    "Unexpected error while calling Sisos CMD API"
                );
            }
        }
        public async Task<BaseResponse<string>> SisosCustomChainAsync(
     SisosCustomChainRequest request,
     CancellationToken cancellationToken)
        {
            var validationResult = await _sisosCustomChainValidator.ValidateAsync(
                request,
                cancellationToken
            );

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(x => x.ErrorMessage)
                    .ToList();

                return BaseResponse<string>.BadRequest(
                    message: "Validation failed",
                    errors: errors
                );
            }

            var bearerToken = _headerHelper.GetBearerToken();

            if (string.IsNullOrWhiteSpace(bearerToken))
            {
                return BaseResponse<string>.Unauthorized(
                    "Bearer token is required."
                );
            }

            var signature = _headerHelper.GetSignature();

            if (string.IsNullOrWhiteSpace(signature))
            {
                return BaseResponse<string>.Unauthorized(
                    "Signature header is required."
                );
            }

            try
            {
                var client = _httpClientFactory.CreateClient("LifePortal");

                var body = new
                {
                    chain = request.Chain,
                    payload = request.Payload
                };

                using var httpRequest = new HttpRequestMessage(
                    HttpMethod.Post,
                    "PortalApi/Sisos/CustomChain"
                );

                httpRequest.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", bearerToken);

                httpRequest.Headers.TryAddWithoutValidation("Signature", signature);

                httpRequest.Content = JsonContent.Create(body);

                using var response = await client.SendAsync(
                    httpRequest,
                    cancellationToken
                );

                var responseBody = await response.Content.ReadAsStringAsync(
                    cancellationToken
                );

                _logger.LogInformation(
                    "Sisos Custom Chain returned status code {StatusCode}",
                    (int)response.StatusCode
                );

                return new BaseResponse<string>(
                    data: responseBody,
                    message: response.IsSuccessStatusCode
                        ? "Sisos custom chain completed successfully"
                        : $"Sisos custom chain failed with status code {(int)response.StatusCode}",
                    statusCode: response.StatusCode
                );
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Sisos custom chain request timed out");

                return BaseResponse<string>.ServiceUnavailable(
                    "Sisos custom chain request timed out"
                );
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to call Sisos Custom Chain API");

                return BaseResponse<string>.BadGateway(
                    "Failed to call Sisos Custom Chain API"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while calling Sisos Custom Chain API");

                return BaseResponse<string>.InternalServerError(
                    "Unexpected error while calling Sisos Custom Chain API"
                );
            }
        }
    }
}
