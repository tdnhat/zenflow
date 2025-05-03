using ZenFlow.Shared.Domain;

namespace Modules.Workflow.DDD.Events
{
    public class WorkflowRunPausedEvent : IDomainEvent
    {
        public Guid RunId { get; }
        public Guid WorkflowId { get; }
        public DateTime OccurredOn { get; }

        public WorkflowRunPausedEvent(Guid runId, Guid workflowId)
        {
            RunId = runId;
            WorkflowId = workflowId;
            OccurredOn = DateTime.UtcNow;
        }
    }
}