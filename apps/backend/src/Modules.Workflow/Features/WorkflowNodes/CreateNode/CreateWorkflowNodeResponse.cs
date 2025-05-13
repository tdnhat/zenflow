namespace Modules.Workflow.Features.WorkflowNodes.CreateNode
{
    public record CreateWorkflowNodeResponse(
        Guid Id, 
        string NodeType,
        string NodeKind, 
        string Label, 
        float X, 
        float Y, 
        string ConfigJson);
} 