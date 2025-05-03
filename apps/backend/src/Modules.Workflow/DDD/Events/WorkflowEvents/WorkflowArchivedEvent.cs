using ZenFlow.Shared.Domain;

namespace Modules.Workflow.DDD.Events
{
    public class WorkflowArchivedEvent : DomainEvent
    {
        public Guid WorkflowId { get; }

        public WorkflowArchivedEvent(Guid workflowId)
        {
            WorkflowId = workflowId;
        }
    }
}