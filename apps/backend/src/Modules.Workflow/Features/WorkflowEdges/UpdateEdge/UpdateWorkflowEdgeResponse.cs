namespace Modules.Workflow.Features.WorkflowEdges.UpdateEdge
{
    public record UpdateWorkflowEdgeResponse(
        Guid Id,
        Guid SourceNodeId,
        Guid TargetNodeId,
        string Label,
        string EdgeType,
        string ConditionJson,
        string SourceHandle,
        string TargetHandle
    );
} 