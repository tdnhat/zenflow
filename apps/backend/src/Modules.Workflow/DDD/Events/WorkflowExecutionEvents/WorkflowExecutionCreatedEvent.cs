using ZenFlow.Shared.Domain;

namespace Modules.Workflow.DDD.Events
{
    public class WorkflowExecutionCreatedEvent : DomainEvent
    {
        public Guid ExecutionId { get; }
        public Guid WorkflowId { get; }

        public WorkflowExecutionCreatedEvent(Guid executionId, Guid workflowId)
        {
            ExecutionId = executionId;
            WorkflowId = workflowId;
        }
    }
}