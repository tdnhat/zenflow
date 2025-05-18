namespace Modules.Workflow.Features.Workflows.Shared
{
    public class WorkflowEdgeDto
    {
        public Guid Id { get; set; }
        public Guid Source { get; set; }
        public Guid Target { get; set; }
        public EdgeConditionDto? Condition { get; set; }
    }
}