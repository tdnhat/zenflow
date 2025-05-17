namespace Modules.Workflow.Features.Workflows.Shared
{
    public class WorkflowEdgeDto
    {
        public string Id { get; set; }
        public string Source { get; set; }
        public string Target { get; set; }
        public EdgeConditionDto Condition { get; set; }
    }
}