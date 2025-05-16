namespace Modules.Workflow.DDD.ValueObjects
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