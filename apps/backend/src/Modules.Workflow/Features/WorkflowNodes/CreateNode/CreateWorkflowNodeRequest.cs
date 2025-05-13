namespace Modules.Workflow.Features.WorkflowNodes.CreateNode
{
    public record CreateWorkflowNodeRequest(
        string NodeType,
        string NodeKind,
        float X,
        float Y,
        string Label,
        string ConfigJson
    );
} 