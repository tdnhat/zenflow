using ZenFlow.Shared.Domain;

namespace Modules.Workflow.DDD.Events
{
    public class WorkflowRunCancelledEvent : DomainEvent
    {
        public Guid RunId { get; }
        public Guid WorkflowId { get; }

        public WorkflowRunCancelledEvent(Guid runId, Guid workflowId)
        {
            RunId = runId;
            WorkflowId = workflowId;
        }
    }
}