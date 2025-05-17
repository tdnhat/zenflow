using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Modules.Workflow.Domain.Interfaces.Core;
using Modules.Workflow.Infrastructure.Services.Activities;
using Modules.Workflow.Infrastructure.Services.AI;
using Modules.Workflow.Infrastructure.Services.Email;
using Modules.Workflow.Infrastructure.Services.Playwright;

namespace Modules.Workflow.Infrastructure.Extensions
{
    public static class WorkflowServiceExtensions
    {
        public static IServiceCollection AddWorkflowExecutionServices(this IServiceCollection services, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
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
    }
}