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
            // Register node types based on actual implemented activities
            RegisterImplementedNodeTypes();
        }

        private void RegisterImplementedNodeTypes()
        {
            // TRIGGER NODES
            RegisterNodeType(new NodeTypeDto(
                "manual-trigger",
                "Manual Trigger",
                "Triggers",
                "TRIGGER",
                "Manually trigger the workflow",
                "👆",
                new List<NodePropertyDto>
                {
                    new NodePropertyDto("name", "Trigger Name", "text", "Manual Start", true, new List<OptionValueDto>()),
                    new NodePropertyDto("description", "Description", "text", "Start workflow manually", false, new List<OptionValueDto>())
                }
            ));

            // BROWSER AUTOMATION NODES
            RegisterNodeType(new NodeTypeDto(
                "navigate",
                "Navigate",
                "Browser Automation",
                "ACTION",
                "Navigate to a URL",
                "🌐",
                new List<NodePropertyDto>
                {
                    new NodePropertyDto("url", "URL", "text", "https://example.com", true, new List<OptionValueDto>()),
                    new NodePropertyDto("timeout", "Timeout (ms)", "number", 30000, false, new List<OptionValueDto>())
                }
            ));

            RegisterNodeType(new NodeTypeDto(
                "click",
                "Click Element",
                "Browser Automation",
                "ACTION",
                "Click on an element in the page",
                "🖱️",
                new List<NodePropertyDto>
                {
                    new NodePropertyDto("selector", "CSS Selector", "text", "", true, new List<OptionValueDto>()),
                    new NodePropertyDto("timeout", "Timeout (ms)", "number", 5000, false, new List<OptionValueDto>())
                }
            ));

            RegisterNodeType(new NodeTypeDto(
                "input-text",
                "Input Text",
                "Browser Automation",
                "ACTION",
                "Enter text into a form field",
                "⌨️",
                new List<NodePropertyDto>
                {
                    new NodePropertyDto("selector", "CSS Selector", "text", "", true, new List<OptionValueDto>()),
                    new NodePropertyDto("text", "Text", "text", "", true, new List<OptionValueDto>()),
                    new NodePropertyDto("clear", "Clear Field First", "boolean", true, false, new List<OptionValueDto>())
                }
            ));

            RegisterNodeType(new NodeTypeDto(
                "wait-for-selector",
                "Wait For Element",
                "Browser Automation",
                "ACTION",
                "Wait for an element to appear in the page",
                "⏳",
                new List<NodePropertyDto>
                {
                    new NodePropertyDto("selector", "CSS Selector", "text", "", true, new List<OptionValueDto>()),
                    new NodePropertyDto("timeout", "Timeout (ms)", "number", 30000, false, new List<OptionValueDto>()),
                    new NodePropertyDto("visible", "Wait Until Visible", "boolean", true, false, new List<OptionValueDto>())
                }
            ));

            RegisterNodeType(new NodeTypeDto(
                "screenshot",
                "Take Screenshot",
                "Browser Automation",
                "ACTION",
                "Capture a screenshot of the current page",
                "📷",
                new List<NodePropertyDto>
                {
                    new NodePropertyDto("filename", "Filename", "text", "screenshot", false, new List<OptionValueDto>()),
                    new NodePropertyDto("fullPage", "Full Page", "boolean", false, false, new List<OptionValueDto>())
                }
            ));

            RegisterNodeType(new NodeTypeDto(
                "crawl-data",
                "Crawl Data",
                "Browser Automation",
                "ACTION",
                "Extract data from web page elements",
                "🕸️",
                new List<NodePropertyDto>
                {
                    new NodePropertyDto("selector", "Root Selector", "text", "", true, new List<OptionValueDto>()),
                    new NodePropertyDto("fields", "Fields to Extract", "json", "[\n  {\n    \"name\": \"title\",\n    \"selector\": \"h1\",\n    \"attribute\": \"innerText\"\n  }\n]", true, new List<OptionValueDto>())
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