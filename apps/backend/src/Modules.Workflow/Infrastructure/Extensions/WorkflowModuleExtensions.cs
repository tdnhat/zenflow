using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.User.Data.Interceptors;
using Modules.Workflow.DDD.Interfaces;
using Elsa.Extensions;
using Modules.Workflow.Infrastructure.Persistence;
using Modules.Workflow.Infrastructure.Persistence.Repositories;
using Modules.Workflow.Infrastructure.Services.NodeManagement;
using Modules.Workflow.Infrastructure.Services.Validation;

namespace Modules.Workflow.Infrastructure.Extensions
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

            services.AddWorkflowOutboxServices();

            // Register the repository and other services
            services.AddScoped<IWorkflowRepository, WorkflowRepository>();
            services.AddScoped<IWorkflowNodeRepository, WorkflowNodeRepository>();
            services.AddScoped<IWorkflowEdgeRepository, WorkflowEdgeRepository>();
            services.AddScoped<IWorkflowExecutionRepository, WorkflowExecutionRepository>();
            services.AddScoped<INodeExecutionRepository, NodeExecutionRepository>();
            services.AddScoped<INodeConfigValidator, NodeConfigValidator>();
            services.AddScoped<INodeTypeRegistry, NodeTypeRegistry>();

            services.AddWorkflowExecutionServices(configuration);

            return services;
        }

        public static WebApplication UseWorkflowModule(this WebApplication app)
        {
            // Use Elsa middleware
            app.UseWorkflows();

            return app;
        }
    }
}
