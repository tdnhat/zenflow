namespace Modules.Workflow.DDD.ValueObjects
{
    public enum WorkflowRunStatus
    {
        QUEUED,
        RUNNING,
        COMPLETED,
        FAILED,
        CANCELLED,
        PAUSED
    }

    // Extension methods for backward compatibility with string values
    public static class WorkflowRunStatusExtensions
    {
        public const string QUEUED_STRING = "QUEUED";
        public const string RUNNING_STRING = "RUNNING";
        public const string COMPLETED_STRING = "COMPLETED";
        public const string FAILED_STRING = "FAILED";
        public const string CANCELLED_STRING = "CANCELLED";
        public const string PAUSED_STRING = "PAUSED";

        public static string ToStringValue(this WorkflowRunStatus status)
        {
            return status switch
            {
                WorkflowRunStatus.QUEUED => QUEUED_STRING,
                WorkflowRunStatus.RUNNING => RUNNING_STRING,
                WorkflowRunStatus.COMPLETED => COMPLETED_STRING,
                WorkflowRunStatus.FAILED => FAILED_STRING,
                WorkflowRunStatus.CANCELLED => CANCELLED_STRING,
                WorkflowRunStatus.PAUSED => PAUSED_STRING,
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            };
        }

        public static WorkflowRunStatus FromString(string status)
        {
            return status switch
            {
                QUEUED_STRING => WorkflowRunStatus.QUEUED,
                RUNNING_STRING => WorkflowRunStatus.RUNNING,
                COMPLETED_STRING => WorkflowRunStatus.COMPLETED,
                FAILED_STRING => WorkflowRunStatus.FAILED,
                CANCELLED_STRING => WorkflowRunStatus.CANCELLED,
                PAUSED_STRING => WorkflowRunStatus.PAUSED,
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            };
        }
    }
}