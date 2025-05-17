using Microsoft.Extensions.Logging;
using Modules.Workflow.Domain.Core;
using Modules.Workflow.Domain.Enums;
using Modules.Workflow.Domain.Interfaces.Core;

namespace Modules.Workflow.Activities
{
    public abstract class BaseActivityExecutor : IActivityExecutor
    {
        protected readonly ILogger<BaseActivityExecutor> _logger;

        public BaseActivityExecutor(ILogger<BaseActivityExecutor> logger)
        {
            _logger = logger;
        }

        public abstract bool CanExecute(string activityType);

        public abstract Task<(ActivityExecutionResult Result, Dictionary<string, object> OutputData)> ExecuteAsync(
            string activityType,
            Dictionary<string, object> activityProperties,
            Dictionary<string, object> inputData,
            WorkflowExecutionContext workflowContext,
            NodeExecutionContext nodeContext,
            CancellationToken cancellationToken = default);

        protected async Task<(ActivityExecutionResult Result, Dictionary<string, object> OutputData)> ExecuteWithErrorHandlingAsync(
            Func<Task<(ActivityExecutionResult, Dictionary<string, object>)>> execution,
            NodeExecutionContext nodeContext)
        {
            try
            {
                return await execution();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing activity {ActivityType}: {ErrorMessage}",
                nodeContext.ActivityType, ex.Message);

                nodeContext.Error = ex.ToString();
                nodeContext.AddLog($"Error: {ex.Message}");

                return (ActivityExecutionResult.Failed, new Dictionary<string, object>
                {
                    ["ErrorMessage"] = ex.Message,
                    ["ErrorDetails"] = ex.ToString()
                });
            }
        }
    }
}