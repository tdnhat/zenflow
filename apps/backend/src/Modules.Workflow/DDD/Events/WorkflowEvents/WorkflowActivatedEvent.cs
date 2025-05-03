using ZenFlow.Shared.Domain;

namespace Modules.Workflow.DDD.Events.WorkflowEdgeEvents
{
    public class WorkflowActivatedEvent : DomainEvent
    {
        public string WorkflowId { get; }
        public DateTime ActivatedAt { get; }
        public string Status { get; } // Use string for backward compatibility with string values

        public WorkflowActivatedEvent(string workflowId, DateTime activatedAt, string status)
        {
            WorkflowId = workflowId;
            ActivatedAt = activatedAt;
            Status = status;
        }
    }
}
