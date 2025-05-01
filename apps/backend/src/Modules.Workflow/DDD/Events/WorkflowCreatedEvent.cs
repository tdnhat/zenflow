using ZenFlow.Shared.Domain;

namespace Modules.Workflow.DDD.Events
{
    public class WorkflowCreatedEvent : DomainEvent
    {
        public Guid WorkflowId { get; }
        public string Name { get; }

        public WorkflowCreatedEvent(Guid workflowId, string name)
        {
            WorkflowId = workflowId;
            Name = name;
        }
    }
}
