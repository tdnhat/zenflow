using Modules.Workflow.DDD.Events;
using Modules.Workflow.DDD.ValueObjects;
using ZenFlow.Shared.Domain;

namespace Modules.Workflow.DDD.Entities
{
    public class WorkflowExecution : Aggregate<Guid>
    {
        public Guid WorkflowId { get; private set; }
        public int WorkflowVersion { get; private set; } = 1;
        public string Status { get; private set; } = WorkflowExecutionStatus.PENDING;
        public DateTime StartedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }
        public string? ErrorMessage { get; private set; }
        public string? ErrorStack { get; private set; }
        public Guid? ErrorNodeId { get; private set; }
        
        // Navigation property
        public Workflow? Workflow { get; private set; }
        
        // Collection of node executions
        public List<NodeExecution> NodeExecutions { get; private set; } = new();

        // Parameterless constructor for EF Core
        public WorkflowExecution() { }

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

            // Raise domain event
            execution.AddDomainEvent(new WorkflowExecutionCreatedEvent(execution.Id, execution.WorkflowId));

            return execution;
        }

        public void Start()
        {
            if (Status != WorkflowExecutionStatus.PENDING)
            {
                throw new InvalidOperationException($"Cannot start workflow execution in {Status} status");
            }

            Status = WorkflowExecutionStatus.RUNNING;
            
            // Raise domain event
            AddDomainEvent(new WorkflowExecutionStartedEvent(Id, WorkflowId));
        }

        public void Complete()
        {
            if (Status != WorkflowExecutionStatus.RUNNING)
            {
                throw new InvalidOperationException($"Cannot complete workflow execution in {Status} status");
            }

            Status = WorkflowExecutionStatus.COMPLETED;
            CompletedAt = DateTime.UtcNow;
            
            // Raise domain event
            AddDomainEvent(new WorkflowExecutionCompletedEvent(Id, WorkflowId));
        }

        public void Fail(string errorMessage, string? errorStack = null, Guid? nodeId = null)
        {
            if (Status != WorkflowExecutionStatus.RUNNING)
            {
                throw new InvalidOperationException($"Cannot fail workflow execution in {Status} status");
            }

            Status = WorkflowExecutionStatus.FAILED;
            CompletedAt = DateTime.UtcNow;
            ErrorMessage = errorMessage;
            ErrorStack = errorStack;
            ErrorNodeId = nodeId;
            
            // Raise domain event
            AddDomainEvent(new WorkflowExecutionFailedEvent(Id, WorkflowId, errorMessage, nodeId));
        }

        public void Cancel()
        {
            if (Status != WorkflowExecutionStatus.RUNNING && Status != WorkflowExecutionStatus.PENDING)
            {
                throw new InvalidOperationException($"Cannot cancel workflow execution in {Status} status");
            }

            Status = WorkflowExecutionStatus.CANCELLED;
            CompletedAt = DateTime.UtcNow;
            
            // Raise domain event
            AddDomainEvent(new WorkflowExecutionCancelledEvent(Id, WorkflowId));
        }
    }
}