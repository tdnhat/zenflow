namespace Modules.Workflow.Domain.Enums
{
    public enum WorkflowNodeStatus
    {
        NotStarted,
        Pending,
        Running,
        Completed,
        Failed,
        Skipped,
        Cancelled
    }
}