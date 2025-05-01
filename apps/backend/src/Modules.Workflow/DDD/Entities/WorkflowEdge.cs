using ZenFlow.Shared.Domain;

namespace Modules.Workflow.DDD.Entities
{
    public class WorkflowEdge : Aggregate<Guid>
    {
        public Guid WorkflowId { get; set; }
        public Guid SourceNodeId { get; set; }
        public Guid TargetNodeId { get; set; }
        public string Label { get; set; } = string.Empty;
        public string ConditionJson { get; set; } = "{}";

        // Parameterless constructor for EF Core
        public WorkflowEdge() { }

        public static WorkflowEdge Create(Guid workflowId, Guid sourceNodeId, Guid targetNodeId, string label, string conditionJson)
        {
            return new WorkflowEdge
            {
                Id = Guid.NewGuid(),
                WorkflowId = workflowId,
                SourceNodeId = sourceNodeId,
                TargetNodeId = targetNodeId,
                Label = label,
                ConditionJson = conditionJson
            };
        }
    }
}
