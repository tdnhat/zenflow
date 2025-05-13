using ZenFlow.Shared.Domain;

namespace Modules.Workflow.DDD.Events
{
    public class WorkflowRunCreatedEvent : IDomainEvent
    {
        public Guid RunId { get; }
        public Guid WorkflowId { get; }
        public DateTime OccurredOn { get; }

        public WorkflowRunCreatedEvent(Guid runId, Guid workflowId)
        {
            RunId = runId;
            WorkflowId = workflowId;
            OccurredOn = DateTime.UtcNow;
        }
    }
}