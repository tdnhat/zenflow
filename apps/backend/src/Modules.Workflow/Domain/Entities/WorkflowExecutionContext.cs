using Modules.Workflow.Domain.Enums;

namespace Modules.Workflow.Domain.Core
{
    public class WorkflowExecutionContext
    {
        public Guid WorkflowInstanceId { get; set; }
        public Guid WorkflowDefinitionId { get; set; }
        public Dictionary<string, object> Variables { get; set; } = new();
        public Dictionary<string, NodeExecutionContext> NodeExecutions { get; set; } = new();
        public WorkflowStatus Status { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? Error { get; set; }

        // Method for variable access and manipulation
        public T GetVariable<T>(string name, T defaultValue = default)
        {
            if (Variables.TryGetValue(name, out var value) && value is T typedValue)
            {
                return typedValue;
            }
            return defaultValue;
        }

        public void SetVariable(string name, object value)
        {
            Variables[name] = value;
        }
    }
}