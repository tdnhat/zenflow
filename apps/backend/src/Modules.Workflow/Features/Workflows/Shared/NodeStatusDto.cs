namespace Modules.Workflow.Features.Workflows.Shared
{
    public class NodeStatusDto
    {
        public string NodeId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ActivityType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public long? DurationMs { get; set; }
        public string? Error { get; set; }
        public List<string> Logs { get; set; } = new();
    }
}