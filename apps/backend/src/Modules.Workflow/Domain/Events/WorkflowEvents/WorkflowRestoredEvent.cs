using ZenFlow.Shared.Domain;

namespace Modules.Workflow.DDD.Events
{
    public class WorkflowRestoredEvent : DomainEvent
    {
        public Guid WorkflowId { get; }

        public WorkflowRestoredEvent(Guid workflowId)
        {
            WorkflowId = workflowId;
        }
    }
}