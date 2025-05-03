using ZenFlow.Shared.Domain;

namespace Modules.Workflow.DDD.Events
{
    public class WorkflowUpdatedEvent : DomainEvent
    {
        public Guid WorkflowId { get; }
        public string Name { get; }

        public WorkflowUpdatedEvent(Guid workflowId, string name)
        {
            WorkflowId = workflowId;
            Name = name;
        }
    }
}
