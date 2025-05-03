namespace Modules.Workflow.DDD.ValueObjects
{
    public enum WorkflowStatus
    {
        DRAFT,
        ACTIVE,
        ARCHIVED
    }

    // Extension methods for backward compatibility with string values
    public static class WorkflowStatusExtensions
    {
        public const string DRAFT_STRING = "DRAFT";
        public const string ACTIVE_STRING = "ACTIVE";
        public const string ARCHIVED_STRING = "ARCHIVED";

        public static string ToStringValue(this WorkflowStatus status)
        {
            return status switch
            {
                WorkflowStatus.DRAFT => DRAFT_STRING,
                WorkflowStatus.ACTIVE => ACTIVE_STRING,
                WorkflowStatus.ARCHIVED => ARCHIVED_STRING,
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            };
        }

        public static WorkflowStatus FromString(string status)
        {
            return status switch
            {
                DRAFT_STRING => WorkflowStatus.DRAFT,
                ACTIVE_STRING => WorkflowStatus.ACTIVE,
                ARCHIVED_STRING => WorkflowStatus.ARCHIVED,
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            };
        }
    }
}
