using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
using Modules.Workflow.Infrastructure.Services.Workflow.Json;
using System;

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
            services.AddScoped<IWorkflowJsonLoader, WorkflowJsonLoader>();
            services.AddSingleton<IPlaywrightFactory, PlaywrightFactory>();

            // Register Playwright services
            services.AddSingleton<IPlaywright>(provider =>
                Playwright.CreateAsync().GetAwaiter().GetResult());
            services.AddSingleton<IPlaywrightService, PlaywrightService>();
            
            // Register activity executors
            // Each executor must be registered both as its concrete type and as IActivityExecutor
            services.AddScoped<PlaywrightActivityExecutor>();
            services.AddScoped<IActivityExecutor, PlaywrightActivityExecutor>();
            
            // Register AI services
            services.AddHttpClient("AIService", client =>
            {
                // Set default base address if not specified in configuration
                var baseUrl = configuration["AI:BaseUrl"];
                if (!string.IsNullOrEmpty(baseUrl))
                {
                    client.BaseAddress = new Uri(baseUrl);
                }
                client.Timeout = TimeSpan.FromSeconds(60);
            });
            
            // Use environment to determine which AI service implementation to use
            services.AddScoped<IAIService>(provider =>
            {
                var env = provider.GetRequiredService<IWebHostEnvironment>();
                if (env.IsDevelopment())
                {
                    // Use mock service in development for testing
                    return provider.GetRequiredService<MockAIService>();
                }
                else
                {
                    // Use real service in production
                    return provider.GetRequiredService<AIService>();
                }
            });
            
            // Register both real and mock implementations
            services.AddScoped<AIService>();
            services.AddScoped<MockAIService>();
            services.AddScoped<AIActivityExecutor>();
            services.AddScoped<IActivityExecutor, AIActivityExecutor>();
            
            // Register Email services
            services.AddScoped<IEmailService, FileEmailService>();
            services.AddScoped<EmailActivityExecutor>();
            services.AddScoped<IActivityExecutor, EmailActivityExecutor>();
            
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
