namespace Modules.Workflow.Features.WorkflowNodes.UpdateNode
{
    public record UpdateWorkflowNodeRequest(
        Guid Id,
        string NodeType,
        string NodeKind,
        float X,
        float Y,
        string Label,
        string ConfigJson
    );
} 