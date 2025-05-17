namespace Modules.Workflow.Features.Workflows.GetWorkflowById
{
    public class InputMappingDto
    {
        public string SourceNodeId { get; set; }
        public string SourceProperty { get; set; }
        public string TargetProperty { get; set; }
    }
}