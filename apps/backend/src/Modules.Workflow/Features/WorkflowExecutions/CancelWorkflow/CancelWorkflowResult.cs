using Elsa.Models;
using Modules.Workflow.DDD.ValueObjects;

namespace Modules.Workflow.Features.WorkflowExecutions.CancelWorkflow
{
    public record CancelWorkflowResult
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
        public string? ExecutionId { get; init; }
        public WorkflowExecutionStatus? Status { get; init; }
    }
}
