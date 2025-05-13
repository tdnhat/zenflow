namespace Modules.Workflow.Features.WorkflowEdges.CreateEdge
{
    public record CreateWorkflowEdgeRequest(
        Guid SourceNodeId,
        Guid TargetNodeId,
        string Label,
        string EdgeType,
        string ConditionJson,
        string SourceHandle,
        string TargetHandle
    );
} 