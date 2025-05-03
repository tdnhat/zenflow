using Elsa.Workflows;
using Elsa.Workflows.Activities;
using Microsoft.Extensions.Logging;

namespace Modules.Workflow.Services.BrowserAutomation.Activities
{
    /// <summary>
    /// Represents a manual trigger activity that serves as a starting point for workflow execution.
    /// </summary>
    public class ManualTriggerActivity : CodeActivity
    {
        private readonly ILogger<ManualTriggerActivity> _logger;

        public ManualTriggerActivity(ILogger<ManualTriggerActivity> logger)
        {
            _logger = logger;
        }

        protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            _logger.LogInformation("Manual trigger activity executed");
            await base.ExecuteAsync(context);
        }
    }
}