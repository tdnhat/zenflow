using Modules.Workflow.Domain.Core;
using Modules.Workflow.Domain.Enums;

namespace Modules.Workflow.Domain.Interfaces.Core
{
    public interface IActivityExecutor
    {
        // Check if this executor can handle the given activity type
        bool CanExecute(string activityType);

        // Execute an activity
        Task<(ActivityExecutionResult Result, Dictionary<string, object> OutputData)> ExecuteAsync(
            string activityType,
            Dictionary<string, object> activityProperties,
            Dictionary<string, object> inputData,
            WorkflowExecutionContext workflowContext,
            NodeExecutionContext nodeContext,
            CancellationToken cancellationToken = default);
    }
}