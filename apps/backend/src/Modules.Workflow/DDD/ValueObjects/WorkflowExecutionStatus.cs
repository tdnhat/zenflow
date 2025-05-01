namespace Modules.Workflow.DDD.ValueObjects
{
    public static class WorkflowExecutionStatus
    {
        public const string PENDING = "PENDING";
        public const string RUNNING = "RUNNING";
        public const string COMPLETED = "COMPLETED";
        public const string FAILED = "FAILED";
        public const string CANCELLED = "CANCELLED";
    }
}