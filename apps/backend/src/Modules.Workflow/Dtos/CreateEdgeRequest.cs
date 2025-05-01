namespace Modules.Workflow.Dtos
{
    public record CreateEdgeRequest(
        Guid SourceNodeId,
        Guid TargetNodeId,
        string Label,
        string EdgeType,
        string ConditionJson,
        string SourceHandle,
        string TargetHandle
    );
}