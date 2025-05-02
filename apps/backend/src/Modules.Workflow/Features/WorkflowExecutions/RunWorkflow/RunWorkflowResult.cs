namespace Modules.Workflow.Features.WorkflowExecutions.RunWorkflow
{
    public class RunWorkflowResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? ExecutionId { get; set; }
        public string? Status { get; set; }
    }
} 