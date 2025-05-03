namespace Modules.Workflow.Features.WorkflowExecutions.CancelWorkflow
{
    public record CancelWorkflowResult
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
        public string? ExecutionId { get; init; }
        public string? Status { get; init; }
    }
}
