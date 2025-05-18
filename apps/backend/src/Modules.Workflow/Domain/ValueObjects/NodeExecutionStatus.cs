namespace Modules.Workflow.Domain.ValueObjects
{
    public enum NodeExecutionStatus
    {
        Pending,
        Ready,
        Running,
        Completed,
        Failed,
        Cancelled,
        WaitingForEvent,
        Skipped
    }
}