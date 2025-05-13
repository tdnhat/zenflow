namespace Modules.Workflow.Features.WorkflowExecutions.Common
{
    public record WorkflowExecutionResponse(
        Guid Id, 
        Guid WorkflowId,
        int WorkflowVersion,
        string Status,
        DateTime StartedAt,
        DateTime? CompletedAt
    );

    public record WorkflowExecutionDetailResponse(
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