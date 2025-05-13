using ZenFlow.Shared.Domain;

namespace Modules.Workflow.DDD.Events
{
    public class WorkflowExecutionCancelledEvent : DomainEvent
    {
        public Guid ExecutionId { get; }
        public Guid WorkflowId { get; }

        public WorkflowExecutionCancelledEvent(Guid executionId, Guid workflowId)
        {
            ExecutionId = executionId;
            WorkflowId = workflowId;
        }
    }
}