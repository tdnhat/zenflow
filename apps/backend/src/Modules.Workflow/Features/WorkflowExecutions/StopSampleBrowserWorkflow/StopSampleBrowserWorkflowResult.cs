namespace Modules.Workflow.Features.WorkflowExecutions.StopSampleBrowserWorkflow
{
    public class StopSampleBrowserWorkflowResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? WorkflowInstanceId { get; set; }
    }
} 