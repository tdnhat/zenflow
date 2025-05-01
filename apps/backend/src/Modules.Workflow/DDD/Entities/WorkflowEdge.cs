using ZenFlow.Shared.Domain;

namespace Modules.Workflow.DDD.Entities
{
    public class WorkflowEdge : Aggregate<Guid>
    {
        public Guid WorkflowId { get; set; }
        public Guid SourceNodeId { get; set; }
        public Guid TargetNodeId { get; set; }
        public string Label { get; set; } = string.Empty;
        public string EdgeType { get; set; } = "default"; // success, failure, condition
        public string ConditionJson { get; set; } = "{}";
        public string SourceHandle { get; set; } = string.Empty; // For nodes with multiple outputs
        public string TargetHandle { get; set; } = string.Empty; // For nodes with multiple inputs

        // Parameterless constructor for EF Core
        public WorkflowEdge() { }

        public static WorkflowEdge Create(
            Guid workflowId, 
            Guid sourceNodeId, 
            Guid targetNodeId, 
            string label, 
            string edgeType,
            string conditionJson,
            string sourceHandle = "",
            string targetHandle = "")
        {
            return new WorkflowEdge
            {
                Id = Guid.NewGuid(),
                WorkflowId = workflowId,
                SourceNodeId = sourceNodeId,
                TargetNodeId = targetNodeId,
                Label = label,
                EdgeType = edgeType,
                ConditionJson = conditionJson,
                SourceHandle = sourceHandle,
                TargetHandle = targetHandle
            };
        }

        // For backward compatibility
        public static WorkflowEdge Create(Guid workflowId, Guid sourceNodeId, Guid targetNodeId, string label, string conditionJson)
        {
            return Create(workflowId, sourceNodeId, targetNodeId, label, "default", conditionJson);
        }
    }
}
