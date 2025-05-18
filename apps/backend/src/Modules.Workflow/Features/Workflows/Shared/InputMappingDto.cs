namespace Modules.Workflow.Features.Workflows.GetWorkflowById
{
    public class InputMappingDto
    {
        public Guid SourceNodeId { get; set; }
        public string SourceProperty { get; set; } = string.Empty;
        public string TargetProperty { get; set; } = string.Empty;
    }
}