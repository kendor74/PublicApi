using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Public.Application.DTO;
using Public.Application.Interface;
using Public.Application.Validators;
using Public.Infrastructure.Common;
using Public.Infrastructure.Configuration;
using Public.Infrastructure.Services;

namespace Public.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var internalApis = configuration.GetSection("InternalApis");
            var lifePortalUrl = internalApis[nameof(InternalApis.LifePortal)];
            var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];

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

            services.AddHttpContextAccessor();
            services.AddScoped<HeaderHelper>();

            return services;
        }
    }
}
