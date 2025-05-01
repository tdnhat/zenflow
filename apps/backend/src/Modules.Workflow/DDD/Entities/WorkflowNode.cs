using Modules.Workflow.DDD.Events;
using ZenFlow.Shared.Domain;

namespace Modules.Workflow.DDD.Entities
{
    public class WorkflowNode : Aggregate<Guid>
    {
        public Guid WorkflowId { get; set; }
        public string NodeType { get; set; } = default!;
        public string Label { get; set; } = string.Empty;
        public float X { get; set; }
        public float Y { get; set; }
        public string ConfigJson { get; set; } = "{}";

        // Parameterless constructor for EF Core
        public WorkflowNode() { }

        public static WorkflowNode Create(Guid workflowId, string nodeType, float x, float y, string label, string configJson)
        {
            var node = new WorkflowNode
            {
                Id = Guid.NewGuid(),
                WorkflowId = workflowId,
                NodeType = nodeType,
                Label = label,
                X = x,
                Y = y,
                ConfigJson = configJson
            };

            // Raise domain event
            node.AddDomainEvent(new WorkflowNodeCreatedEvent(node.Id, node.WorkflowId, node.NodeType));

            return node;
        }

        public void Update(float x, float y, string label, string configJson)
        {
            X = x;
            Y = y;
            Label = label;
            ConfigJson = configJson;

            // Raise domain event
            AddDomainEvent(new WorkflowNodeUpdatedEvent(Id, WorkflowId, NodeType));
        }

        public void MarkAsDeleted()
        {
            // Raise domain event for deletion
            AddDomainEvent(new WorkflowNodeDeletedEvent(Id, WorkflowId));
        }
    }
}
