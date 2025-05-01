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
            return new WorkflowNode
            {
                Id = Guid.NewGuid(),
                WorkflowId = workflowId,
                NodeType = nodeType,
                Label = label,
                X = x,
                Y = y,
                ConfigJson = configJson
            };
        }
    }
}
