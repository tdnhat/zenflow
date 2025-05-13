namespace Modules.Workflow.Features.WorkflowEdges.CreateEdge
{
    public record CreateWorkflowEdgeResponse(
        Guid Id,
        Guid SourceNodeId, 
        Guid TargetNodeId, 
        string Label,
        string EdgeType,
        string ConditionJson,
        string SourceHandle,
        string TargetHandle);
} 