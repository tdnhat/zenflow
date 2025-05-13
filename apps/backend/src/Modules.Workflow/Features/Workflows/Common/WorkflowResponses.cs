namespace Modules.Workflow.Features.Workflows.Common
{
    public record WorkflowResponse(
        Guid Id, 
        string Name,
        string Description, 
        string Status, 
        DateTime? CreatedAt,
        DateTime? LastModifiedAt);
    
    public record WorkflowDetailResponse(
        Guid Id, 
        string Name, 
        string Description, 
        string Status,
        DateTime? CreatedAt,
        DateTime? LastModifiedAt,
        IEnumerable<WorkflowNodeResponse> Nodes,
        IEnumerable<WorkflowEdgeResponse> Edges);
        
    public record WorkflowNodeResponse(
        Guid Id, 
        string NodeType,
        string NodeKind, 
        string Label, 
        float X, 
        float Y, 
        string ConfigJson);
        
    public record WorkflowEdgeResponse(
        Guid Id, 
        Guid SourceNodeId, 
        Guid TargetNodeId, 
        string Label,
        string EdgeType,
        string ConditionJson,
        string SourceHandle,
        string TargetHandle);
} 