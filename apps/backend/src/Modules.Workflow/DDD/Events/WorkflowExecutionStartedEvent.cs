using ZenFlow.Shared.Domain;

namespace Modules.Workflow.DDD.Events
{
    public class WorkflowExecutionStartedEvent : IDomainEvent
    {
        public Guid ExecutionId { get; }
        public Guid WorkflowId { get; }
        public DateTime OccurredOn { get; }

        public WorkflowExecutionStartedEvent(Guid executionId, Guid workflowId)
        {
            ExecutionId = executionId;
            WorkflowId = workflowId;
            OccurredOn = DateTime.UtcNow;
        }
    }
}