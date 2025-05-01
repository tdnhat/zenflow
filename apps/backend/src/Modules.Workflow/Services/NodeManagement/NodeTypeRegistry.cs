using Modules.Workflow.Dtos;

namespace Modules.Workflow.Services.NodeManagement
{
    public interface INodeTypeRegistry
    {
        IEnumerable<NodeTypeDto> GetAllNodeTypes();
        IEnumerable<string> GetCategories();
        IEnumerable<NodeTypeDto> GetNodeTypesByCategory(string category);
        NodeTypeDto? GetNodeTypeByType(string type);
    }

    public class NodeTypeRegistry : INodeTypeRegistry
    {
        private readonly Dictionary<string, NodeTypeDto> _nodeTypes = new();

        public NodeTypeRegistry()
        {
            // Register built-in node types
            RegisterCoreNodeTypes();
        }

        private void RegisterCoreNodeTypes()
        {
            // TRIGGER NODES
            RegisterNodeType(new NodeTypeDto(
                "webhook",
                "Webhook Trigger",
                "Triggers",
                "TRIGGER",
                "Start workflow when a webhook URL is called",
                "🌐",
                new List<NodePropertyDto>
                {
                    new NodePropertyDto("path", "Path", "text", "", true, new List<OptionValueDto>()),
                    new NodePropertyDto("method", "HTTP Method", "select", "POST", true,
                        new List<OptionValueDto> {
                            new OptionValueDto("GET", "GET"),
                            new OptionValueDto("POST", "POST"),
                            new OptionValueDto("PUT", "PUT"),
                            new OptionValueDto("DELETE", "DELETE")
                        })
                }
            ));

            RegisterNodeType(new NodeTypeDto(
                "timer",
                "Timer Trigger",
                "Triggers",
                "TRIGGER",
                "Start workflow at specified intervals",
                "⏰",
                new List<NodePropertyDto>
                {
                    new NodePropertyDto("schedule", "Schedule (CRON)", "text", "0 * * * *", true, new List<OptionValueDto>()),
                    new NodePropertyDto("timezone", "Timezone", "text", "UTC", false, new List<OptionValueDto>())
                }
            ));

            // ACTION NODES
            RegisterNodeType(new NodeTypeDto(
                "http",
                "HTTP Request",
                "Actions",
                "ACTION",
                "Make HTTP requests to external APIs",
                "🔗",
                new List<NodePropertyDto>
                {
                    new NodePropertyDto("url", "URL", "text", "", true, new List<OptionValueDto>()),
                    new NodePropertyDto("method", "Method", "select", "GET", true,
                        new List<OptionValueDto> {
                            new OptionValueDto("GET", "GET"),
                            new OptionValueDto("POST", "POST"),
                            new OptionValueDto("PUT", "PUT"),
                            new OptionValueDto("DELETE", "DELETE"),
                            new OptionValueDto("PATCH", "PATCH")
                        }),
                    new NodePropertyDto("headers", "Headers", "json", "{}", false, new List<OptionValueDto>()),
                    new NodePropertyDto("body", "Body", "json", "{}", false, new List<OptionValueDto>())
                }
            ));

            RegisterNodeType(new NodeTypeDto(
                "transform",
                "Transform Data",
                "Actions",
                "ACTION",
                "Transform data using JavaScript expressions",
                "🔄",
                new List<NodePropertyDto>
                {
                    new NodePropertyDto("script", "JavaScript", "code", "// Example: \nreturn { \n  result: input.value * 2 \n};", true, new List<OptionValueDto>())
                }
            ));

            // CONDITION NODES
            RegisterNodeType(new NodeTypeDto(
                "condition",
                "Condition",
                "Control Flow",
                "CONDITION",
                "Branch workflow based on conditions",
                "🔀",
                new List<NodePropertyDto>
                {
                    new NodePropertyDto("condition", "JavaScript Condition", "code", "// Example: \nreturn input.value > 10;", true, new List<OptionValueDto>())
                }
            ));

            // LOOP NODES
            RegisterNodeType(new NodeTypeDto(
                "foreach",
                "For Each",
                "Control Flow",
                "LOOP",
                "Loop through items in an array",
                "🔄",
                new List<NodePropertyDto>
                {
                    new NodePropertyDto("items", "Items Path", "text", "items", true, new List<OptionValueDto>()),
                    new NodePropertyDto("maxIterations", "Max Iterations", "number", 100, false, new List<OptionValueDto>())
                }
            ));

            // CONNECTOR NODES
            RegisterNodeType(new NodeTypeDto(
                "database",
                "Database",
                "Connectors",
                "CONNECTOR",
                "Connect to a database",
                "🗄️",
                new List<NodePropertyDto>
                {
                    new NodePropertyDto("connectionString", "Connection String", "text", "", true, new List<OptionValueDto>()),
                    new NodePropertyDto("query", "SQL Query", "code", "SELECT * FROM table", true, new List<OptionValueDto>()),
                    new NodePropertyDto("parameters", "Parameters", "json", "{}", false, new List<OptionValueDto>())
                }
            ));
        }

        public void RegisterNodeType(NodeTypeDto nodeType)
        {
            _nodeTypes[nodeType.Type] = nodeType;
        }

        public IEnumerable<NodeTypeDto> GetAllNodeTypes()
        {
            return _nodeTypes.Values;
        }

        public IEnumerable<string> GetCategories()
        {
            return _nodeTypes.Values.Select(n => n.Category).Distinct();
        }

        public IEnumerable<NodeTypeDto> GetNodeTypesByCategory(string category)
        {
            return _nodeTypes.Values.Where(n => n.Category == category);
        }

        public NodeTypeDto? GetNodeTypeByType(string type)
        {
            _nodeTypes.TryGetValue(type, out var nodeType);
            return nodeType;
        }
    }
}