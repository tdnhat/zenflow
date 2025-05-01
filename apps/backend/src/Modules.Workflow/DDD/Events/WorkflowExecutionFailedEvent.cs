using ZenFlow.Shared.Domain;

namespace Modules.Workflow.DDD.Events
{
    public class WorkflowExecutionFailedEvent : IDomainEvent
    {
        public Guid ExecutionId { get; }
        public Guid WorkflowId { get; }
        public string ErrorMessage { get; }
        public Guid? ErrorNodeId { get; }
        public DateTime OccurredOn { get; }

        public WorkflowExecutionFailedEvent(Guid executionId, Guid workflowId, string errorMessage, Guid? errorNodeId = null)
        {
            ExecutionId = executionId;
            WorkflowId = workflowId;
            ErrorMessage = errorMessage;
            ErrorNodeId = errorNodeId;
            OccurredOn = DateTime.UtcNow;
        }
    }
}