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
        public NodeExecutionStatus Status { get; private set; } = NodeExecutionStatus.PENDING;
        public DateTime? StartedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }
        public string? InputJson { get; private set; }
        public string? OutputJson { get; private set; }
        public string? Error { get; private set; }
        public long? DurationMs { get; private set; }
        
        // Navigation properties
        public WorkflowExecution? WorkflowExecution { get; private set; }
        public WorkflowNode? Node { get; private set; }

        // Parameterless constructor for EF Core
        public NodeExecution() { }

        public static NodeExecution Create(Guid workflowExecutionId, Guid nodeId, Dictionary<string, object>? input = null)
        {
            return new NodeExecution
            {
                Id = Guid.NewGuid(),
                WorkflowExecutionId = workflowExecutionId,
                NodeId = nodeId,
                Status = NodeExecutionStatus.PENDING,
                InputJson = input != null ? JsonSerializer.Serialize(input) : null
            };
        }

        public void Start()
        {
            if (Status != NodeExecutionStatus.PENDING)
            {
                throw new InvalidOperationException($"Cannot start node execution in {Status} status");
            }

            Status = NodeExecutionStatus.RUNNING;
            StartedAt = DateTime.UtcNow;
        }

        public void Complete(Dictionary<string, object>? output = null)
        {
            if (Status != NodeExecutionStatus.RUNNING)
            {
                throw new InvalidOperationException($"Cannot complete node execution in {Status} status");
            }

            Status = NodeExecutionStatus.COMPLETED;
            CompletedAt = DateTime.UtcNow;
            OutputJson = output != null ? JsonSerializer.Serialize(output) : null;
            CalculateDuration();
        }

        public void Fail(string error)
        {
            if (Status != NodeExecutionStatus.RUNNING)
            {
                throw new InvalidOperationException($"Cannot fail node execution in {Status} status");
            }

            Status = NodeExecutionStatus.FAILED;
            CompletedAt = DateTime.UtcNow;
            Error = error;
            CalculateDuration();
        }

        public void Skip()
        {
            if (Status != NodeExecutionStatus.PENDING)
            {
                throw new InvalidOperationException($"Cannot skip node execution in {Status} status");
            }

            Status = NodeExecutionStatus.SKIPPED;
            CompletedAt = DateTime.UtcNow;
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