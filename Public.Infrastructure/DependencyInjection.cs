using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Public.Application.Interface;
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
                if (Uri.TryCreate(lifePortalUrl, UriKind.Absolute, out var baseAddress))
                {
                    client.BaseAddress = baseAddress;
                }
            });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials());
            });


            return services;
        }
    }
}
