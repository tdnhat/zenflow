using Modules.Workflow.Domain.Enums;

namespace Modules.Workflow.Domain.Core
{
    public class NodeExecutionContext
    {
        public string NodeId { get; set; } = string.Empty;
        public string ActivityType { get; set; } = string.Empty;
        public Dictionary<string, object> InputData { get; set; } = new();
        public Dictionary<string, object> OutputData { get; set; } = new();
        public WorkflowNodeStatus Status { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? Error { get; set; }
        public List<string> Logs { get; set; } = new();

        public void AddLog(string message)
        {
            Logs.Add($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {message}");
        }
    }
}