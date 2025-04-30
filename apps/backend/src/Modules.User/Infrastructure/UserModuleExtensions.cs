using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.User.Data;
using Modules.User.Data.Interceptors;
using Modules.User.DDD.Interfaces;
using Modules.User.Repositories;
using Modules.User.Services;

namespace Modules.User.Infrastructure
{
    public static class UserModuleExtensions
    {
        public static IServiceCollection AddUserModule(this IServiceCollection services, IConfiguration configuration)
        {
            // Add services for the User module

            // Add Endpoints for the User module

            // Application Use Case Services

            // Data - Infrastructure Services
            services.AddDbContext<UserDbContext>((sp, options) =>
            {
                options
                    .UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
                    .AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
            });

            services.AddScoped<AuditInterceptor>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddHttpClient<IKeycloakAdminService, KeycloakAdminService>();
            services.AddHttpClient<IKeycloakTokenService, KeycloakTokenService>();

            return services;
        }

        public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder endpoints)
        {
            // Map endpoints for the User module
            var apiGroup = endpoints.MapGroup("/api/v1/users");
            apiGroup.MapCarter();

            return endpoints;
        }

        public static WebApplication UseUserModule(this WebApplication app)
        {

            return app;
        }
    }
}
