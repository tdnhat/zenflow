namespace Modules.Workflow.Domain.Enums
{
    public enum WorkflowStatus
    {
        NotStarted,
        Running,
        Completed,
        Failed,
        Cancelled,
        Suspended
    }
}