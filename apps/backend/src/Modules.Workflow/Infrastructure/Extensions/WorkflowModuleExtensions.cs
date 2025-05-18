using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Playwright;
using Modules.Workflow.Domain.Activities;
using Modules.Workflow.Domain.Interfaces.Core;
using Modules.Workflow.Infrastructure.Persistence;
using Modules.Workflow.Infrastructure.Persistence.Repositories;
using Modules.Workflow.Infrastructure.Services.Activities;
using Modules.Workflow.Infrastructure.Services.AI;
using Modules.Workflow.Infrastructure.Services.Email;
using Modules.Workflow.Infrastructure.Services.Playwright;
using Modules.Workflow.Infrastructure.Services.Workflow;
using Modules.Workflow.Infrastructure.Services.Workflow.Activities.Email;
using Modules.Workflow.Infrastructure.Services.Workflow.Json;
using Modules.Workflow.Infrastructure.Services.Workflow.Playwright;
using System;

namespace Modules.Workflow.Infrastructure.Extensions
{
    public static class WorkflowModuleExtensions
    {
        public static IServiceCollection AddWorkflowModule(this IServiceCollection services, IConfiguration configuration)
        {
            // Register database
            services.AddWorkflowDatabase(configuration);
            
            // Register core workflow services
            services.AddWorkflowCore();
            
            // Register activity executors
            services.AddActivityExecutors();
            
            // Register external services
            services.AddPlaywrightServices();
            services.AddAIServices(configuration);
            services.AddEmailServices();
            
            return services;
        }
        
        private static IServiceCollection AddWorkflowDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<WorkflowDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
                
            services.AddScoped<IWorkflowRepository, WorkflowRepository>();
            services.AddScoped<IWorkflowInstanceRepository, WorkflowInstanceRepository>();
            
            return services;
        }
        
        private static IServiceCollection AddWorkflowCore(this IServiceCollection services)
        {
            services.AddScoped<IWorkflowEngine, WorkflowEngine>();
            services.AddScoped<IWorkflowJsonLoader, WorkflowJsonLoader>();
            
            return services;
        }
        
        private static IServiceCollection AddActivityExecutors(this IServiceCollection services)
        {
            // Register Email activities
            services.RegisterActivity<EmailActivityExecutor>(new SendEmailActivityDescriptor());
            
            // Register Playwright activities
            services.AddScoped<PlaywrightActivityExecutor>();
            services.AddScoped<IActivityExecutor, PlaywrightActivityExecutor>();
            
            // Register AI activities
            services.AddScoped<AIActivityExecutor>();
            services.AddScoped<IActivityExecutor, AIActivityExecutor>();
            
            return services;
        }
        
        private static IServiceCollection AddPlaywrightServices(this IServiceCollection services)
        {
            services.AddSingleton<IPlaywrightFactory, PlaywrightFactory>();
            services.AddSingleton<IPlaywright>(provider =>
                Playwright.CreateAsync().GetAwaiter().GetResult());
            services.AddSingleton<IPlaywrightService, PlaywrightService>();
            
            return services;
        }
        
        private static IServiceCollection AddAIServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient("AIService", client =>
            {
                var baseUrl = configuration["AI:BaseUrl"];
                if (!string.IsNullOrEmpty(baseUrl))
                {
                    client.BaseAddress = new Uri(baseUrl);
                }
                client.Timeout = TimeSpan.FromSeconds(60);
            });
            
            // Register both implementations
            services.AddScoped<AIService>();
            services.AddScoped<MockAIService>();
            
            // Use environment to determine which implementation to use
            services.AddScoped<IAIService>(provider =>
            {
                var env = provider.GetRequiredService<IWebHostEnvironment>();
                return env.IsDevelopment()
                    ? provider.GetRequiredService<MockAIService>()
                    : provider.GetRequiredService<AIService>();
            });
            
            return services;
        }
        
        private static IServiceCollection AddEmailServices(this IServiceCollection services)
        {
            services.AddScoped<IEmailService, FileEmailService>();
            
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
