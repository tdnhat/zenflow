using ZenFlow.Shared.Domain;

namespace Modules.Workflow.DDD.Entities
{
    public class WorkflowEdge : Entity<Guid>
    {
        public Guid WorkflowId { get; private set; }
        public Guid SourceNodeId { get; private set; }
        public Guid TargetNodeId { get; private set; }
        public string Label { get; private set; } = string.Empty;
        public string EdgeType { get; private set; } = "default"; // success, failure, condition
        public string ConditionJson { get; private set; } = "{}";
        public string SourceHandle { get; private set; } = string.Empty; // For nodes with multiple outputs
        public string TargetHandle { get; private set; } = string.Empty; // For nodes with multiple inputs

        // Navigation property for the Aggregate Root
        public Workflow? Workflow { get; private set; }

        // Parameterless constructor for EF Core
        private WorkflowEdge() { }

        internal static WorkflowEdge Create(
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
        internal static WorkflowEdge Create(Guid workflowId, Guid sourceNodeId, Guid targetNodeId, string label, string conditionJson)
        {
            return Create(workflowId, sourceNodeId, targetNodeId, label, "default", conditionJson);
        }

        internal void Update(string label, string edgeType, string conditionJson, string sourceHandle, string targetHandle)
        {
            Label = label;
            EdgeType = edgeType;
            ConditionJson = conditionJson;
            SourceHandle = sourceHandle;
            TargetHandle = targetHandle;
        }
    }
}
