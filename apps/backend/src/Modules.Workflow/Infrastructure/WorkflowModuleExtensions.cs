using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.User.Data.Interceptors;
using Modules.Workflow.Data;
using Modules.Workflow.DDD.Interfaces;
using Modules.Workflow.Repositories;

namespace Modules.Workflow.Infrastructure
{
    public static class WorkflowModuleExtensions
    {
        public static IServiceCollection AddWorkflowModule(this IServiceCollection services, IConfiguration configuration)
        {
            // Register the DbContext with the connection string from configuration
            services.AddDbContext<WorkflowDbContext>((sp, options) =>
            {
                options
                    .UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
                    .AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
            });

            // Register the repository and other services
            services.AddScoped<IWorkflowRepository, WorkflowRepository>();

            return services;
        }

        public static IEndpointRouteBuilder MapWorkflowEndpoints(this IEndpointRouteBuilder endpoints)
        {
            // Map endpoints for the User module
            var apiGroup = endpoints.MapGroup("/api/v1/workflows");
            apiGroup.MapCarter();

            return endpoints;
        }


        public static WebApplication UseWorkflowModule(this WebApplication app)
        {
            return app;
        }
    }
}
