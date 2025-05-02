using Elsa.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Modules.Workflow.Services.BrowserAutomation;
using Modules.Workflow.Services.BrowserAutomation.Activities;
using Modules.Workflow.DDD.Interfaces;
using Modules.Workflow.Workflows;

namespace Modules.Workflow.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWorkflowServices(this IServiceCollection services)
        {
            // Register browser automation services
            services.AddSingleton<IBrowserAutomation, BrowserAutomation>();
            services.AddSingleton<IBrowserSessionManager, BrowserSessionManager>();
            services.AddScoped<IWorkflowLifecycleHandler, WorkflowCleanupHandler>();

            // Add Elsa services
            services.AddElsa(elsa =>
            {
                // Register browser automation activities
                elsa.AddActivity<NavigateActivity>()
                    .AddActivity<ClickActivity>()
                    .AddActivity<InputTextActivity>()
                    .AddActivity<WaitForSelectorActivity>()
                    .AddActivity<ExtractDataActivity>()
                    .AddActivity<ScreenshotActivity>();

                // Register workflows
                elsa.AddWorkflow<SampleBrowserWorkflow>();
            });

            return services;
        }
    }
}