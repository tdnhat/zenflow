using Elsa.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Modules.Workflow.Services.BrowserAutomation;
using Modules.Workflow.Services.BrowserAutomation.Activities;
using Modules.Workflow.DDD.Interfaces;

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
                   .AddActivity<ScreenshotActivity>();

            });

            return services;
        }
    }
}