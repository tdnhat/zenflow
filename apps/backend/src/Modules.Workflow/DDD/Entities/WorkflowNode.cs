using Modules.Workflow.DDD.Events;
using ZenFlow.Shared.Domain;

namespace Modules.Workflow.DDD.Entities
{
    public class WorkflowNode : Entity<Guid>
    {
        public Guid WorkflowId { get; private set; }
        public string NodeType { get; private set; } = default!;
        public string NodeKind { get; private set; } = "ACTION"; // Default to ACTION for backward compatibility
        public string Label { get; private set; } = string.Empty;
        public float X { get; private set; }
        public float Y { get; private set; }
        public string ConfigJson { get; private set; } = "{}";

        // Navigation property for the Aggregate Root
        public Workflow? Workflow { get; private set; }

        // Parameterless constructor for EF Core
        private WorkflowNode() { }

        internal static WorkflowNode Create(Guid workflowId, string nodeType, string nodeKind, float x, float y, string label, string configJson)
        {
            var node = new WorkflowNode
            {
                Id = Guid.NewGuid(),
                WorkflowId = workflowId,
                NodeType = nodeType,
                NodeKind = nodeKind,
                Label = label,
                X = x,
                Y = y,
                ConfigJson = configJson
            };

            return node;
        }

        // For backward compatibility
        internal static WorkflowNode Create(Guid workflowId, string nodeType, float x, float y, string label, string configJson)
        {
            return Create(workflowId, nodeType, "ACTION", x, y, label, configJson);
        }

        internal void Update(float x, float y, string label, string configJson)
        {
            X = x;
            Y = y;
            Label = label;
            ConfigJson = configJson;
        }
    }
}
