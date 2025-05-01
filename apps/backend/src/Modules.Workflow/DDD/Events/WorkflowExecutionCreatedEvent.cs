using ZenFlow.Shared.Domain;

namespace Modules.Workflow.DDD.Events
{
    public class WorkflowExecutionCreatedEvent : IDomainEvent
    {
        public Guid ExecutionId { get; }
        public Guid WorkflowId { get; }
        public DateTime OccurredOn { get; }

        public WorkflowExecutionCreatedEvent(Guid executionId, Guid workflowId)
        {
            ExecutionId = executionId;
            WorkflowId = workflowId;
            OccurredOn = DateTime.UtcNow;
        }
    }
}