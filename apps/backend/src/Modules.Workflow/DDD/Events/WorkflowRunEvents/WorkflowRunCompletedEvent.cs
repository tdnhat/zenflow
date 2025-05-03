using ZenFlow.Shared.Domain;

namespace Modules.Workflow.DDD.Events
{
    public class WorkflowRunCompletedEvent : DomainEvent
    {
        public Guid RunId { get; }
        public Guid WorkflowId { get; }

        public WorkflowRunCompletedEvent(Guid runId, Guid workflowId)
        {
            RunId = runId;
            WorkflowId = workflowId;
        }
    }
}