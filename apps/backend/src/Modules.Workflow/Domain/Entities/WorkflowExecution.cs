using Modules.Workflow.DDD.Events;
using Modules.Workflow.DDD.ValueObjects;
using System.Text.Json;
using ZenFlow.Shared.Domain;

namespace Modules.Workflow.DDD.Entities
{
    public class WorkflowExecution : Aggregate<Guid>
    {
        public Guid WorkflowId { get; private set; }
        public int WorkflowVersion { get; private set; } = 1;
        public WorkflowExecutionStatus Status { get; private set; } = WorkflowExecutionStatus.PENDING;
        public DateTime StartedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }
        public string? ErrorMessage { get; private set; }
        public string? ErrorStack { get; private set; }
        public Guid? ErrorNodeId { get; private set; }
        public string? ExternalWorkflowId { get; private set; }
        public string? OutputData { get; private set; }

        public Workflow? Workflow { get; private set; }
        public List<NodeExecution> NodeExecutions { get; private set; } = new();

        private WorkflowExecution() { }

        public static WorkflowExecution Create(Guid workflowId, int workflowVersion = 1)
        {
            var execution = new WorkflowExecution
            {
                Id = Guid.NewGuid(),
                WorkflowId = workflowId,
                WorkflowVersion = workflowVersion,
                Status = WorkflowExecutionStatus.PENDING,
                StartedAt = DateTime.UtcNow
            };
            execution.AddDomainEvent(new WorkflowExecutionCreatedEvent(execution.Id, execution.WorkflowId));
            return execution;
        }

        public void Start()
        {
            if (Status != WorkflowExecutionStatus.PENDING)
            {
                throw new InvalidOperationException($"Cannot start workflow execution in {Status.ToStringValue()} status");
            }
            Status = WorkflowExecutionStatus.RUNNING;
            AddDomainEvent(new WorkflowExecutionStartedEvent(Id, WorkflowId));
        }

        public void Complete(string? outputData = null)
        {
            if (Status != WorkflowExecutionStatus.RUNNING)
            {
                throw new InvalidOperationException($"Cannot complete workflow execution in {Status.ToStringValue()} status");
            }
            Status = WorkflowExecutionStatus.COMPLETED;
            CompletedAt = DateTime.UtcNow;
            if (outputData != null) OutputData = outputData;
            AddDomainEvent(new WorkflowExecutionCompletedEvent(Id, WorkflowId));
        }

        public void Fail(string? errorMessage, string? errorStack, Guid? errorNodeId = null)
        {
            Status = WorkflowExecutionStatus.FAILED;
            ErrorMessage = errorMessage;
            ErrorStack = errorStack;
            ErrorNodeId = errorNodeId;
            CompletedAt = DateTime.UtcNow;
            AddDomainEvent(new WorkflowExecutionFailedEvent(Id, WorkflowId, errorMessage ?? "Unknown error"));
        }

        public void SetExternalWorkflowId(string externalWorkflowId)
        {
            ExternalWorkflowId = externalWorkflowId;
        }

        public void StoreOutput(string outputData)
        {
            OutputData = outputData;
        }

        public void Cancel(string? reason = null)
        {
            if (Status == WorkflowExecutionStatus.COMPLETED || Status == WorkflowExecutionStatus.FAILED)
            {
                throw new InvalidOperationException($"Cannot cancel workflow execution in {Status.ToStringValue()} status");
            }
            Status = WorkflowExecutionStatus.CANCELLED;
            CompletedAt = DateTime.UtcNow;
            ErrorMessage = reason ?? "Cancelled by user";
            var runningNodes = NodeExecutions.Where(n => n.Status == NodeExecutionStatus.Running).ToList();
            foreach (var nodeExecution in runningNodes)
            {
                nodeExecution.Fail("Cancelled due to workflow execution cancellation");
            }
            AddDomainEvent(new WorkflowExecutionCancelledEvent(Id, WorkflowId));
        }
    }
}