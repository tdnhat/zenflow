using Modules.Workflow.DDD.Events;
using ZenFlow.Shared.Domain;

namespace Modules.Workflow.DDD.Entities
{
    public class WorkflowNode : Entity<Guid>
    {
        public Guid WorkflowId { get; private set; }
        public string NodeType { get; private set; } = default!;
        public string ActivityType { get; private set; } = default!;
        public string Label { get; private set; } = string.Empty;
        public float X { get; private set; }
        public float Y { get; private set; }
        public string ActivityPropertiesJson { get; private set; } = "{}"; // JSON string for activity properties
        public string BackendIntegrationJson { get; private set; } = "{}";
        public string InputMappingsJson { get; private set; } = "[]";
        public string OutputMappingsJson { get; private set; } = "[]";

        // Navigation property for the Aggregate Root
        public Workflow? Workflow { get; private set; }

        // Parameterless constructor for EF Core
        private WorkflowNode() { }

        internal static WorkflowNode Create(
            Guid workflowId,
            string nodeType,
            string activityType,
            float x,
            float y,
            string label,
            string activityPropertiesJson,
            string backendIntegrationJson = "{}",
            string inputMappingsJson = "[]",
            string outputMappingsJson = "[]")
        {
            return new WorkflowNode
            {
                Id = Guid.NewGuid(),
                WorkflowId = workflowId,
                NodeType = nodeType,
                ActivityType = activityType,
                Label = label,
                X = x,
                Y = y,
                ActivityPropertiesJson = activityPropertiesJson,
                BackendIntegrationJson = backendIntegrationJson,
                InputMappingsJson = inputMappingsJson,
                OutputMappingsJson = outputMappingsJson
            };
        }

        internal void Update(
            float x,
            float y,
            string label,
            string activityPropertiesJson,
            string backendIntegrationJson = "{}",
            string inputMappingsJson = "[]",
            string outputMappingsJson = "[]")
        {
            X = x;
            Y = y;
            Label = label;
            ActivityPropertiesJson = activityPropertiesJson;
            BackendIntegrationJson = backendIntegrationJson;
            InputMappingsJson = inputMappingsJson;
            OutputMappingsJson = outputMappingsJson;
        }

        internal void Update(
            string nodeType,
            string activityType,
            float x,
            float y,
            string label,
            string activityPropertiesJson,
            string backendIntegrationJson = "{}",
            string inputMappingsJson = "[]",
            string outputMappingsJson = "[]")
        {
            NodeType = nodeType;
            ActivityType = activityType;
            X = x;
            Y = y;
            Label = label;
            ActivityPropertiesJson = activityPropertiesJson;
            BackendIntegrationJson = backendIntegrationJson;
            InputMappingsJson = inputMappingsJson;
            OutputMappingsJson = outputMappingsJson;
        }
    }
}
