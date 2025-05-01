using System.Text.Json;

namespace Modules.Workflow.Services.NodeManagement
{
    public interface INodeConfigValidator
    {
        bool ValidateConfig(string nodeType, string configJson, out List<string> errors);
    }

    public class NodeConfigValidator : INodeConfigValidator
    {
        private readonly IServiceProvider _serviceProvider;

        public NodeConfigValidator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public bool ValidateConfig(string nodeType, string configJson, out List<string> errors)
        {
            errors = new List<string>();

            try
            {
                // Get validator for specific node type if available
                var validatorType = Type.GetType($"Modules.Workflow.NodeTypes.{nodeType}ConfigValidator");
                if (validatorType != null && _serviceProvider.GetService(validatorType) is INodeTypeConfigValidator validator)
                {
                    // Use specific validator
                    return validator.Validate(configJson, out errors);
                }

                // Basic validation - check if it's valid JSON
                var config = JsonSerializer.Deserialize<Dictionary<string, object>>(configJson);

                // If we got here, the JSON is valid
                return true;
            }
            catch (JsonException ex)
            {
                errors.Add($"Invalid JSON configuration: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                errors.Add($"Validation error: {ex.Message}");
                return false;
            }
        }
    }

    // Interface for node-type specific validators
    public interface INodeTypeConfigValidator
    {
        bool Validate(string configJson, out List<string> errors);
    }
}