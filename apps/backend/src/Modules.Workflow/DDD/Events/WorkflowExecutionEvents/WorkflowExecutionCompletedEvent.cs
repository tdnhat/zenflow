using ZenFlow.Shared.Domain;

namespace Modules.Workflow.DDD.Events
{
    public class WorkflowExecutionCompletedEvent : DomainEvent
    {
        public Guid ExecutionId { get; }
        public Guid WorkflowId { get; }

        public WorkflowExecutionCompletedEvent(Guid executionId, Guid workflowId)
        {
            ExecutionId = executionId;
            WorkflowId = workflowId;
        }
    }
}