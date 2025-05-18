namespace Modules.Workflow.Features.Workflows.Shared
{
    public class WorkflowRunStatusDto
    {
        public Guid WorkflowRunId { get; set; }
        public Guid WorkflowId { get; set; }
        public string WorkflowName { get; set; }
        public string Status { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<NodeStatusDto> Nodes { get; set; } = new();
    }
}