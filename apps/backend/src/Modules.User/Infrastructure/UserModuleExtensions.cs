using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Modules.User.Data;
using Modules.User.Data.Interceptors;
using Modules.User.DDD.Interfaces;
using Modules.User.Repositories;
using Modules.User.Services;
using Polly;
using Polly.Extensions.Http;
using ZenFlow.Shared.Configurations;

namespace Modules.User.Infrastructure
{
    public static class UserModuleExtensions
    {
        public static IServiceCollection AddUserModule(this IServiceCollection services, IConfiguration configuration)
        {
            // Register Keycloak settings from configuration
            services.AddOptions<KeycloakSettings>()
                .Bind(configuration.GetSection("Keycloak"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddSingleton(sp => sp.GetRequiredService<IOptions<KeycloakSettings>>().Value);

            // Register the Keycloak token service with retry and circuit breaker policies
            services.AddHttpClient<KeycloakTokenService>()
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy());

            // Data infrastructure services
            services.AddDbContext<UserDbContext>((sp, options) =>
            {
                options
                    .UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
                    .AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
            });

            //services.AddScoped<AuditInterceptor>();
            services.AddScoped<IUserRepository, UserRepository>();

            // Register Keycloak services with proper dependency injection
            // 1. First register HttpClient for each service that needs it
            services.AddHttpClient<KeycloakTokenService>()
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy());

            services.AddHttpClient<KeycloakHttpClient>()
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy());

            services.AddHttpClient<KeycloakRoleService>()
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy());

            services.AddHttpClient<KeycloakAdminService>()
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy());

            // 2. Then register each service with its interface
            services.AddScoped<IKeycloakTokenService, KeycloakTokenService>();
            services.AddScoped<IKeycloakHttpClient, KeycloakHttpClient>();
            services.AddScoped<IKeycloakRoleService, KeycloakRoleService>();
            services.AddScoped<IKeycloakAdminService, KeycloakAdminService>();

            return services;
        }

        public static WebApplication UseUserModule(this WebApplication app)
        {
            return app;
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
        }
    }
}
