namespace Modules.Workflow.DDD.ValueObjects
{
    public enum NodeExecutionStatus
    {
        PENDING,
        RUNNING,
        COMPLETED,
        FAILED,
        SKIPPED
    }

    // Extension methods for backward compatibility with string values
    public static class NodeExecutionStatusExtensions
    {
        public const string PENDING_STRING = "PENDING";
        public const string RUNNING_STRING = "RUNNING";
        public const string COMPLETED_STRING = "COMPLETED";
        public const string FAILED_STRING = "FAILED";
        public const string SKIPPED_STRING = "SKIPPED";

        public static string ToStringValue(this NodeExecutionStatus status)
        {
            return status switch
            {
                NodeExecutionStatus.PENDING => PENDING_STRING,
                NodeExecutionStatus.RUNNING => RUNNING_STRING,
                NodeExecutionStatus.COMPLETED => COMPLETED_STRING,
                NodeExecutionStatus.FAILED => FAILED_STRING,
                NodeExecutionStatus.SKIPPED => SKIPPED_STRING,
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            };
        }

        public static NodeExecutionStatus FromString(string status)
        {
            return status switch
            {
                PENDING_STRING => NodeExecutionStatus.PENDING,
                RUNNING_STRING => NodeExecutionStatus.RUNNING,
                COMPLETED_STRING => NodeExecutionStatus.COMPLETED,
                FAILED_STRING => NodeExecutionStatus.FAILED,
                SKIPPED_STRING => NodeExecutionStatus.SKIPPED,
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            };
        }
    }
}