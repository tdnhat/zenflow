using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Modules.Workflow.Domain.Interfaces.Core;
using Modules.Workflow.Infrastructure.Persistence;
using Modules.Workflow.Infrastructure.Persistence.Repositories;
using Modules.Workflow.Infrastructure.Services.Activities;
using Modules.Workflow.Infrastructure.Services.AI;
using Modules.Workflow.Infrastructure.Services.Email;
using Modules.Workflow.Infrastructure.Services.Playwright;
using Modules.Workflow.Infrastructure.Services.Workflow;
using Modules.Workflow.Infrastructure.Services.Workflow.Playwright;

namespace Modules.Workflow.Infrastructure.Extensions
{
    public static class WorkflowModuleExtensions
    {
        public static IServiceCollection AddWorkflowModule(this IServiceCollection services, IConfiguration configuration)
        {
            // Register the DbContext with the connection string from configuration
            services.AddDbContext<WorkflowDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            // Register the repository and other services
            services.AddScoped<IWorkflowRepository, WorkflowRepository>();
            services.AddScoped<IWorkflowInstanceRepository, WorkflowInstanceRepository>();
            services.AddScoped<IWorkflowEngine, WorkflowEngine>();
            services.AddSingleton<IPlaywrightFactory, PlaywrightFactory>();

            // Register Playwright services
            services.AddSingleton<IPlaywright>(provider =>
                Playwright.CreateAsync().GetAwaiter().GetResult());
            services.AddSingleton<IPlaywrightService, PlaywrightService>();
            services.AddScoped<PlaywrightActivityExecutor>();

            // Register AI services
            services.AddHttpClient<IAIService, AIService>();
            services.AddScoped<AIActivityExecutor>();

            // Register Email services
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<EmailActivityExecutor>();

            return services;
        }

        public static WebApplication UseWorkflowModule(this WebApplication app)
        {
            // Create the workflow schema if it doesn't exist
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkflowDbContext>();
                dbContext.Database.ExecuteSqlRaw("CREATE SCHEMA IF NOT EXISTS workflow;");
            }
            
            return app;
        }
    }
}
