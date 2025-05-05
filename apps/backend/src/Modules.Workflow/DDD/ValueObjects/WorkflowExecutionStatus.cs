namespace Modules.Workflow.DDD.ValueObjects
{
    public enum WorkflowExecutionStatus
    {
        PENDING,
        RUNNING,
        COMPLETED,
        FAILED,
        CANCELLED
    }

    // Extension methods for backward compatibility with string values
    public static class WorkflowExecutionStatusExtensions
    {
        public const string PENDING_STRING = "PENDING";
        public const string RUNNING_STRING = "RUNNING";
        public const string COMPLETED_STRING = "COMPLETED";
        public const string FAILED_STRING = "FAILED";
        public const string CANCELLED_STRING = "CANCELLED";

        public static string ToStringValue(this WorkflowExecutionStatus status)
        {
            return status switch
            {
                WorkflowExecutionStatus.PENDING => PENDING_STRING,
                WorkflowExecutionStatus.RUNNING => RUNNING_STRING,
                WorkflowExecutionStatus.COMPLETED => COMPLETED_STRING,
                WorkflowExecutionStatus.FAILED => FAILED_STRING,
                WorkflowExecutionStatus.CANCELLED => CANCELLED_STRING,
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            };
        }

        public static WorkflowExecutionStatus FromString(string status)
        {
            return status switch
            {
                PENDING_STRING => WorkflowExecutionStatus.PENDING,
                RUNNING_STRING => WorkflowExecutionStatus.RUNNING,
                COMPLETED_STRING => WorkflowExecutionStatus.COMPLETED,
                FAILED_STRING => WorkflowExecutionStatus.FAILED,
                CANCELLED_STRING => WorkflowExecutionStatus.CANCELLED,
                _ => WorkflowExecutionStatus.PENDING
            };
        }
    }
}