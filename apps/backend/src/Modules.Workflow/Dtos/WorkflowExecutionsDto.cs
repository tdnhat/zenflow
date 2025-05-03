namespace Modules.Workflow.Dtos
{
    public record WorkflowExecutionDto(
        Guid Id, 
        Guid WorkflowId,
        int WorkflowVersion,
        string Status,
        DateTime StartedAt,
        DateTime? CompletedAt
    );

    public record WorkflowExecutionDetailDto(
        Guid Id, 
        Guid WorkflowId,
        int WorkflowVersion,
        string Status,
        DateTime StartedAt,
        DateTime? CompletedAt,
        string? ErrorMessage,
        string? ErrorStack,
        Guid? ErrorNodeId,
        string? ErrorNodeType,
        string? ExternalWorkflowId,
        string? OutputData
    );
}
