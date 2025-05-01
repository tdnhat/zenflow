using ZenFlow.Shared.Domain;

namespace Modules.Workflow.DDD.Events
{
    public class WorkflowRunFailedEvent : IDomainEvent
    {
        public Guid RunId { get; }
        public Guid WorkflowId { get; }
        public string Error { get; }
        public DateTime OccurredOn { get; }

        public WorkflowRunFailedEvent(Guid runId, Guid workflowId, string error)
        {
            RunId = runId;
            WorkflowId = workflowId;
            Error = error;
            OccurredOn = DateTime.UtcNow;
        }
    }
}