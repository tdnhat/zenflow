using ZenFlow.Shared.Domain;

namespace Modules.Workflow.DDD.Events.WorkflowEdgeEvents
{
    public class WorkflowEdgeDeletedEvent : DomainEvent
    {
        public Guid EdgeId { get; }
        public Guid SourceNodeId { get; }
        public Guid TargetNodeId { get; }
        public Guid WorkflowId { get; }

        public WorkflowEdgeDeletedEvent(Guid edgeId, Guid sourceNodeId, Guid targetNodeId, Guid workflowId)
        {
            EdgeId = edgeId;
            SourceNodeId = sourceNodeId;
            TargetNodeId = targetNodeId;
            WorkflowId = workflowId;
        }
    }
}
