using Modules.Workflow.DDD.Events;
using Modules.Workflow.DDD.ValueObjects;
using ZenFlow.Shared.Domain;
using System.Text.Json;

namespace Modules.Workflow.DDD.Entities
{
    public class NodeExecution : Entity<Guid>
    {
        public Guid WorkflowExecutionId { get; private set; }
        public Guid NodeId { get; private set; }
        public NodeExecutionStatus Status { get; private set; } = NodeExecutionStatus.Pending;
        public DateTime? StartedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }
        public DateTime? LastUpdatedAt { get; private set; }
        public string? InputJson { get; private set; }
        public string? OutputJson { get; private set; }
        public string? Error { get; private set; }
        public int? ProgressPercentage { get; private set; }
        public string LogsJson { get; private set; } = "[]";
        public long? DurationMs { get; private set; }

        public WorkflowExecution? WorkflowExecution { get; private set; }
        public WorkflowNode? Node { get; private set; }

        private NodeExecution() { }

        public static NodeExecution Create(Guid workflowExecutionId, Guid nodeId, Dictionary<string, object>? input = null)
        {
            return new NodeExecution
            {
                Id = Guid.NewGuid(),
                WorkflowExecutionId = workflowExecutionId,
                NodeId = nodeId,
                Status = NodeExecutionStatus.Pending,
                InputJson = input != null ? JsonSerializer.Serialize(input) : null,
                LastUpdatedAt = DateTime.UtcNow
            };
        }

        public void Start()
        {
            if (Status != NodeExecutionStatus.Pending && Status != NodeExecutionStatus.Ready)
            {
                throw new InvalidOperationException($"Cannot start node execution in {Status} status");
            }
            Status = NodeExecutionStatus.Running;
            StartedAt = DateTime.UtcNow;
            LastUpdatedAt = StartedAt;
        }

        public void Complete(Dictionary<string, object>? output = null)
        {
            if (Status != NodeExecutionStatus.Running)
            {
                throw new InvalidOperationException($"Cannot complete node execution in {Status} status");
            }
            Status = NodeExecutionStatus.Completed;
            CompletedAt = DateTime.UtcNow;
            OutputJson = output != null ? JsonSerializer.Serialize(output) : null;
            CalculateDuration();
            LastUpdatedAt = CompletedAt;
        }

        public void Fail(string error)
        {
            if (Status != NodeExecutionStatus.Running)
            {
                throw new InvalidOperationException($"Cannot fail node execution in {Status} status");
            }
            Status = NodeExecutionStatus.Failed;
            CompletedAt = DateTime.UtcNow;
            Error = error;
            CalculateDuration();
            LastUpdatedAt = CompletedAt;
        }

        public void Skip()
        {
            if (Status != NodeExecutionStatus.Pending)
            {
                throw new InvalidOperationException($"Cannot skip node execution in {Status} status");
            }
            Status = NodeExecutionStatus.Skipped;
            CompletedAt = DateTime.UtcNow;
            LastUpdatedAt = CompletedAt;
        }

        public void UpdateProgress(int progress)
        {
            if (Status != NodeExecutionStatus.Running)
            {
                throw new InvalidOperationException($"Cannot update progress in {Status} status");
            }
            ProgressPercentage = progress;
            LastUpdatedAt = DateTime.UtcNow;
        }

        private void CalculateDuration()
        {
            if (StartedAt.HasValue && CompletedAt.HasValue)
            {
                var duration = CompletedAt.Value - StartedAt.Value;
                DurationMs = (long)duration.TotalMilliseconds;
            }
        }
    }
}