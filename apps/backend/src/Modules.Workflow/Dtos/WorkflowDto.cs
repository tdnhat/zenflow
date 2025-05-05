namespace Modules.Workflow.Dtos
{
    public record WorkflowDto(
        Guid Id, 
        string Name,
        string Description, 
        string Status, 
        DateTime? CreatedAt,
        DateTime? LastModifiedAt);
    
    public record WorkflowDetailDto(
        Guid Id, 
        string Name, 
        string Description, 
        string Status,
        DateTime? CreatedAt,
        DateTime? LastModifiedAt,
        IEnumerable<WorkflowNodeDto> Nodes,
        IEnumerable<WorkflowEdgeDto> Edges);
        
    public record WorkflowNodeDto(
        Guid Id, 
        string NodeType,
        string NodeKind, 
        string Label, 
        float X, 
        float Y, 
        string ConfigJson);
        
    public record WorkflowEdgeDto(
        Guid Id, 
        Guid SourceNodeId, 
        Guid TargetNodeId, 
        string Label,
        string EdgeType,
        string ConditionJson,
        string SourceHandle,
        string TargetHandle);
}
