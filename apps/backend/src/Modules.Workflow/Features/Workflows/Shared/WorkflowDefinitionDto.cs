namespace Modules.Workflow.Features.Workflows.Shared
{
    public class WorkflowDefinitionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Version { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<WorkflowNodeDto> Nodes { get; set; } = new();
        public List<WorkflowEdgeDto> Edges { get; set; } = new();
    }
}