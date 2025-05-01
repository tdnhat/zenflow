namespace Modules.Workflow.DDD.ValueObjects
{
    public static class WorkflowRunStatus
    {
        public const string QUEUED = "QUEUED";
        public const string RUNNING = "RUNNING";
        public const string COMPLETED = "COMPLETED";
        public const string FAILED = "FAILED";
        public const string CANCELLED = "CANCELLED";
        public const string PAUSED = "PAUSED";
    }
}