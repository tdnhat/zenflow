using Modules.Workflow.Features.Workflows.GetWorkflowById;

namespace Modules.Workflow.Features.Workflows.Shared
{
    public class WorkflowNodeDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ActivityType { get; set; }
        public Dictionary<string, object> ActivityProperties { get; set; } = new();
        public List<InputMappingDto> InputMappings { get; set; } = new();
        public List<OutputMappingDto> OutputMappings { get; set; } = new();
        public NodePositionDto Position { get; set; } = new();
    }
}