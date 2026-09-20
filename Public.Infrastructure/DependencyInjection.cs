using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Public.Application.DTO;
using Public.Application.DTO.Mcp;
using Public.Application.Interface;
using Public.Application.Validators;
using Public.Infrastructure.Common;
using Public.Infrastructure.Configuration;
using Public.Infrastructure.Service;
using Public.Infrastructure.Services;
namespace Public.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var internalApis = configuration.GetSection("InternalApis");
            var lifePortalUrl = internalApis[nameof(InternalApis.LifePortal)];
            var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

            services.Configure<InternalApis>(internalApis);
            services.AddHttpClient<IPortalServices, PortalService>();
            services.AddHttpClient("LifePortal", client =>
            {
                var baseUrl = configuration["InternalApis:LifePortal"];

                if (string.IsNullOrWhiteSpace(baseUrl))
                {
                    throw new InvalidOperationException(
                        "InternalApis:LifePortal is not configured."
                    );
                }

                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(60);
            });

            services.AddHttpClient("McpServer",client =>
            {
                var url = configuration["InternalApis:McpServer"];

                if (string.IsNullOrWhiteSpace(url))
                {
                    throw new InvalidOperationException(
                        "InternalApis:McpServer is not configured.");
                }

                client.BaseAddress = new Uri(url);

                client.Timeout = Timeout.InfiniteTimeSpan;
            });


            services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials());
            });

            services.AddScoped<IPortalServices, PortalService>();
            services.AddScoped<IValidator<SisosLoginRequest>, SisosLoginRequestValidator>();
            services.AddScoped<IValidator<SisosCmdRequest>, SisosCmdRequestValidator>();
            services.AddScoped<IValidator<SisosCustomChainRequest>, SisosCustomChainRequestValidator>();
            services.AddScoped<IValidator<SmsOTPRequest>, SmsOTPRequestValidator>();
            services.AddScoped<IValidator<SmsMessageRequest>, SmsMessageRequestValidator>();
            services.AddScoped<IValidator<McpApiKeyHeader>, McpRequestValidator>();
            services.AddScoped<IMcpService, McpService>();

            services.AddHttpContextAccessor();
            services.AddScoped<HeaderHelper>();

            services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer(
                    (document, context, cancellationToken) =>
                    {
                        document.Components ??= new OpenApiComponents();

                        document.Components.SecuritySchemes ??=
                            new Dictionary<string, IOpenApiSecurityScheme>();

                        document.Components.SecuritySchemes["BearerAuth"] =
                            new OpenApiSecurityScheme
                            {
                                Type = SecuritySchemeType.Http,
                                Scheme = "bearer",
                                BearerFormat = "JWT",
                                In = ParameterLocation.Header,
                                Name = "Authorization",
                                Description =
                                    "Enter the JWT access token."
                            };

                        return Task.CompletedTask;
                    });

                options.AddOperationTransformer(
                    (operation, context, cancellationToken) =>
                    {
                        var endpointMetadata =
                            context.Description.ActionDescriptor.EndpointMetadata;

                        var allowsAnonymous = endpointMetadata
                            .OfType<IAllowAnonymous>()
                            .Any();

                        var requiresAuthorization = endpointMetadata
                            .OfType<IAuthorizeData>()
                            .Any();

                        if (allowsAnonymous || !requiresAuthorization)
                        {
                            return Task.CompletedTask;
                        }

                        if (context.Document is null)
                        {
                            return Task.CompletedTask;
                        }

                        operation.Security ??=
                            new List<OpenApiSecurityRequirement>();

                        var bearerRequirement =
                            new OpenApiSecurityRequirement
                            {
                                [
                                    new OpenApiSecuritySchemeReference(
                                        "BearerAuth",
                                        context.Document)
                                ] = new List<string>()
                            };

                        operation.Security.Add(bearerRequirement);

                        return Task.CompletedTask;
                    });
            });
           

            


            return services;
        }
    }
}
