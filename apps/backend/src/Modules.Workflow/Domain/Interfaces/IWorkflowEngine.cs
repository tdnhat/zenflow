using Modules.Workflow.Domain.Core;

namespace Modules.Workflow.Domain.Interfaces.Core
{
    public interface IWorkflowEngine
    {
        // Start a new workflow instance
        Task<Guid> StartWorkflowAsync(
            Guid workflowDefinitionId,
            Dictionary<string, object>? initialVariables = null,
            CancellationToken cancellationToken = default);

        // Resume a suspended workflow at a specific node
        Task ResumeWorkflowAsync(
            Guid workflowInstanceId,
            Guid nodeId,
            Dictionary<string, object>? outputData = null,
            CancellationToken cancellationToken = default);

        // Cancel a running workflow
        Task CancelWorkflowAsync(
            Guid workflowInstanceId,
            CancellationToken cancellationToken = default);

        // Get the current state of a workflow
        Task<WorkflowExecutionContext> GetWorkflowStateAsync(
            Guid workflowInstanceId,
            CancellationToken cancellationToken = default);
    }
}