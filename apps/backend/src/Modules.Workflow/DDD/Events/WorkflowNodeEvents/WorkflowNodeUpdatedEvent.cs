using ZenFlow.Shared.Domain;

namespace Modules.Workflow.DDD.Events
{
    public class WorkflowNodeUpdatedEvent : DomainEvent
    {
        public Guid NodeId { get; }
        public Guid WorkflowId { get; }
        public string NodeType { get; }

        public WorkflowNodeUpdatedEvent(Guid nodeId, Guid workflowId, string nodeType)
        {
            NodeId = nodeId;
            WorkflowId = workflowId;
            NodeType = nodeType;
        }
    }
}