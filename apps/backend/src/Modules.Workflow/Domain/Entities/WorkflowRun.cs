using Modules.Workflow.DDD.Events;
using Modules.Workflow.DDD.ValueObjects;
using ZenFlow.Shared.Domain;

namespace Modules.Workflow.DDD.Entities
{
    public class WorkflowRun : Aggregate<Guid>
    {
        public Guid WorkflowId { get; private set; }
        public WorkflowRunStatus Status { get; private set; } = WorkflowRunStatus.QUEUED;
        public DateTime StartedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }
        public string? Error { get; private set; }
        
        // Navigation property - optional depending on your EF setup
        public Workflow? Workflow { get; private set; }
        
        // Collection of node executions
        public List<NodeExecution> NodeExecutions { get; private set; } = new();

        // Parameterless constructor for EF Core
        public WorkflowRun() { }

        public static WorkflowRun Create(Guid workflowId)
        {
            var workflowRun = new WorkflowRun
            {
                Id = Guid.NewGuid(),
                WorkflowId = workflowId,
                Status = WorkflowRunStatus.QUEUED,
                StartedAt = DateTime.UtcNow
            };

            // Raise domain event
            workflowRun.AddDomainEvent(new WorkflowRunCreatedEvent(workflowRun.Id, workflowRun.WorkflowId));

            return workflowRun;
        }

        public void Start()
        {
            if (Status != WorkflowRunStatus.QUEUED)
            {
                throw new InvalidOperationException($"Cannot start workflow run in {Status} status");
            }

            Status = WorkflowRunStatus.RUNNING;
            
            // Raise domain event
            AddDomainEvent(new WorkflowRunStartedEvent(Id, WorkflowId));
        }

        public void Complete()
        {
            if (Status != WorkflowRunStatus.RUNNING)
            {
                throw new InvalidOperationException($"Cannot complete workflow run in {Status} status");
            }

            Status = WorkflowRunStatus.COMPLETED;
            CompletedAt = DateTime.UtcNow;
            
            // Raise domain event
            AddDomainEvent(new WorkflowRunCompletedEvent(Id, WorkflowId));
        }

        public void Fail(string error)
        {
            if (Status != WorkflowRunStatus.RUNNING)
            {
                throw new InvalidOperationException($"Cannot fail workflow run in {Status} status");
            }

            Status = WorkflowRunStatus.FAILED;
            CompletedAt = DateTime.UtcNow;
            Error = error;
            
            // Raise domain event
            AddDomainEvent(new WorkflowRunFailedEvent(Id, WorkflowId, error));
        }

        public void Cancel()
        {
            if (Status != WorkflowRunStatus.RUNNING && Status != WorkflowRunStatus.PAUSED)
            {
                throw new InvalidOperationException($"Cannot cancel workflow run in {Status} status");
            }

            Status = WorkflowRunStatus.CANCELLED;
            CompletedAt = DateTime.UtcNow;
            
            // Raise domain event
            AddDomainEvent(new WorkflowRunCancelledEvent(Id, WorkflowId));
        }

        public void Pause()
        {
            if (Status != WorkflowRunStatus.RUNNING)
            {
                throw new InvalidOperationException($"Cannot pause workflow run in {Status} status");
            }

            Status = WorkflowRunStatus.PAUSED;
            
            // Raise domain event
            AddDomainEvent(new WorkflowRunPausedEvent(Id, WorkflowId));
        }

        public void Resume()
        {
            if (Status != WorkflowRunStatus.PAUSED)
            {
                throw new InvalidOperationException($"Cannot resume workflow run in {Status} status");
            }

            Status = WorkflowRunStatus.RUNNING;
            
            // Raise domain event
            AddDomainEvent(new WorkflowRunResumedEvent(Id, WorkflowId));
        }
    }
}