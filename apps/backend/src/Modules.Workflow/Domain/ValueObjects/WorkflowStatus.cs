namespace Modules.Workflow.Domain.ValueObjects
{
    public enum WorkflowStatus
    {
        Draft,
        Active,
        Archived
    }

    // Extension methods for backward compatibility with string values
    public static class WorkflowStatusExtensions
    {
        public static string ToStringValue(this WorkflowStatus status)
        {
            return status.ToString();
        }

        public static WorkflowStatus FromString(string value)
        {
            return Enum.Parse<WorkflowStatus>(value);
        }
    }
}
