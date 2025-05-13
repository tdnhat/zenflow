using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Interfaces;

namespace Modules.Workflow.Infrastructure.Services.BrowserAutomation
{
    public class WorkflowCleanupHandler : IWorkflowLifecycleHandler
    {
        private readonly IBrowserSessionManager _browserSessionManager;
        private readonly ILogger<WorkflowCleanupHandler> _logger;

        public WorkflowCleanupHandler(IBrowserSessionManager browserSessionManager, ILogger<WorkflowCleanupHandler> logger)
        {
            _browserSessionManager = browserSessionManager;
            _logger = logger;
        }

        public async ValueTask WorkflowCompletedAsync(WorkflowCompletedContext context, CancellationToken cancellationToken)
        {
            await CleanupResourcesAsync(context.WorkflowExecution.Id.ToString());
        }

        public async ValueTask WorkflowFaultedAsync(WorkflowFaultedContext context, CancellationToken cancellationToken)
        {
            await CleanupResourcesAsync(context.WorkflowExecution.Id.ToString());
        }

        public async ValueTask WorkflowCancelledAsync(WorkflowCancelledContext context, CancellationToken cancellationToken)
        {
            await CleanupResourcesAsync(context.WorkflowExecution.Id.ToString());
        }

        private async Task CleanupResourcesAsync(string workflowInstanceId)
        {
            try
            {
                await _browserSessionManager.CleanupWorkflowResourcesAsync(workflowInstanceId);
                _logger.LogInformation("Cleaned up browser resources for workflow {workflowInstanceId}", workflowInstanceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up browser resources for workflow {workflowInstanceId}", workflowInstanceId);
            }
        }
    }
}
