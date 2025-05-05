using Elsa.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Modules.Workflow.Services.BrowserAutomation;
using Modules.Workflow.Services.BrowserAutomation.Activities;
using Modules.Workflow.DDD.Interfaces;
using Modules.Workflow.Features.WorkflowExecutions.RunWorkflow.ActivityMappers;

namespace Modules.Workflow.Infrastructure.Extensions
{
    public static class WorkflowExecutionServices
    {
        public static IServiceCollection AddWorkflowExecutionServices(this IServiceCollection services, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            // Register browser automation services
            services.AddSingleton<IBrowserAutomation, BrowserAutomation>();
            services.AddSingleton<IBrowserSessionManager, BrowserSessionManager>();
            services.AddScoped<IWorkflowLifecycleHandler, WorkflowCleanupHandler>();
            
            // Register activity mappers
            services.AddScoped<IActivityMapperFactory, ActivityMapperFactory>();
            services.AddScoped<IActivityMapper, BrowserActivityMapper>();
            services.AddScoped<IActivityMapper, DefaultActivityMapper>();

            // Add Elsa services
            services.AddElsa(elsa =>
            {
                // Enable HTTP activity for API integration scenarios
                elsa.UseHttp();

                // Enable JavaScript support for expressions
                elsa.UseJavaScript();

                // Register browser automation activities
                elsa.AddActivity<NavigateActivity>()
                   .AddActivity<ClickActivity>()
                   .AddActivity<InputTextActivity>()
                   .AddActivity<WaitForSelectorActivity>()
                   .AddActivity<CrawlDataActivity>()
                   .AddActivity<ScreenshotActivity>()
                   .AddActivity<ManualTriggerActivity>();

            });

            return services;
        }
    }
}