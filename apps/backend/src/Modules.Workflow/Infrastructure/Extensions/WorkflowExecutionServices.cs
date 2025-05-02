using Elsa.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Modules.Workflow.Services.BrowserAutomation;
using Modules.Workflow.Services.BrowserAutomation.Activities;
using Modules.Workflow.DDD.Interfaces;
using Modules.Workflow.Workflows;

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
            
            // Configure SampleBrowserWorkflow options
            services.Configure<SampleBrowserWorkflowOptions>(options => {
                // These are the default values, can be overridden by configuration
                options.SearchEngineUrl = "https://www.google.com";
                options.SearchTerm = "Elsa Workflows";
                options.SearchInputSelector = "textarea[name='q']";
                options.SearchButtonSelector = "input[name='btnK']";
                options.SearchResultsSelector = "div[role='gridcell']";
                options.ExtractDataSelector = "div[role='gridcell']";
                options.EnableScreenshots = false;
                options.TypeDelay = 50;
            });

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
                   .AddActivity<ExtractDataActivity>()
                   .AddActivity<ScreenshotActivity>();

                // Register workflows
                elsa.AddWorkflow<SampleBrowserWorkflow>();
            });

            return services;
        }
    }
}