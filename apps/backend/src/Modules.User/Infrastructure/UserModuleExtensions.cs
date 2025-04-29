using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.User.Data;
using Modules.User.Data.Interceptors;

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

            return services;
        }

        public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder endpoints)
        {
            
            return endpoints;
        }

        public static WebApplication UseUserModule(this WebApplication app)
        {
            
            return app;
        }
    }
}
