namespace Modules.Workflow.Features.WorkflowExecutions.GetWorkflowExecutions
{
    public class WorkflowExecutionsFilterRequest
    {
        public Guid? WorkflowId { get; set; }
        public string? Status { get; set; }
    }
}