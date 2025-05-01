using ZenFlow.Shared.Domain;

namespace Modules.Workflow.DDD.Events
{
    public class WorkflowNodeDeletedEvent : DomainEvent
    {
        public Guid NodeId { get; }
        public Guid WorkflowId { get; }

        public WorkflowNodeDeletedEvent(Guid nodeId, Guid workflowId)
        {
            NodeId = nodeId;
            WorkflowId = workflowId;
        }
    }
}