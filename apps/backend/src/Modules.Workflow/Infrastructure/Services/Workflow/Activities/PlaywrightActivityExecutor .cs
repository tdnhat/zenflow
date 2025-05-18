using Microsoft.Extensions.Logging;
using Modules.Workflow.Activities;
using Modules.Workflow.Domain.Core;
using Modules.Workflow.Domain.Enums;
using Modules.Workflow.Domain.Interfaces.Core;
using System.Text.Json;
using System.Linq;

namespace Modules.Workflow.Infrastructure.Services.Activities
{
    public class PlaywrightActivityExecutor : BaseActivityExecutor
    {
        private readonly IPlaywrightService _playwrightService;

        public PlaywrightActivityExecutor(
            ILogger<PlaywrightActivityExecutor> logger,
            IPlaywrightService playwrightService) : base(logger)
        {
            _playwrightService = playwrightService;
        }

        public override bool CanExecute(string activityType)
        {
            return activityType.StartsWith("ZenFlow.Activities.Playwright.");
        }

        public override async Task<(ActivityExecutionResult Result, Dictionary<string, object> OutputData)> ExecuteAsync(
            string activityType,
            Dictionary<string, object> activityProperties,
            Dictionary<string, object> inputData,
            WorkflowExecutionContext workflowContext,
            NodeExecutionContext nodeContext,
            CancellationToken cancellationToken = default)
        {
            // Log all activity properties for debugging
            nodeContext.AddLog($"Activity properties: {JsonSerializer.Serialize(activityProperties)}");
            nodeContext.AddLog($"Input data: {JsonSerializer.Serialize(inputData)}");
            
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                nodeContext.AddLog($"Executing Playwright activity: {activityType}");

                switch (activityType)
                {
                    case "ZenFlow.Activities.Playwright.GetElementAttributeActivity":
                        return await ExecuteGetElementAttributeActivityAsync(
                            activityProperties, inputData, nodeContext);

                    case "ZenFlow.Activities.Playwright.ExtractTextFromElementActivity":
                        return await ExecuteExtractTextFromElementAsync(
                            activityProperties, inputData, nodeContext);

                    default:
                        throw new NotSupportedException($"Unsupported activity type: {activityType}");
                }
            }, nodeContext);
        }
        private async Task<(ActivityExecutionResult, Dictionary<string, object>)> ExecuteGetElementAttributeActivityAsync(
            Dictionary<string, object> activityProperties,
            Dictionary<string, object> inputData,
            NodeExecutionContext nodeContext)
        {
            // Extract activity properties
            var targetUrl = GetPropertyValue<string>(activityProperties, inputData, "targetUrl");
            var elementSelector = GetPropertyValue<string>(activityProperties, inputData, "elementSelector");
            var attributeName = GetPropertyValue<string>(activityProperties, inputData, "attributeName");

            nodeContext.AddLog($"Getting attribute '{attributeName}' from element '{elementSelector}' on page '{targetUrl}'");

            // Execute the Playwright operation
            var attributeValue = await _playwrightService.GetElementAttributeAsync(
                targetUrl, elementSelector, attributeName);

            nodeContext.AddLog($"Attribute value: {attributeValue}");

            // Return the result
            return (ActivityExecutionResult.Completed, new Dictionary<string, object>
            {
                ["attributeValue"] = attributeValue
            });
        }

        private async Task<(ActivityExecutionResult, Dictionary<string, object>)> ExecuteExtractTextFromElementAsync(
            Dictionary<string, object> activityProperties,
            Dictionary<string, object> inputData,
            NodeExecutionContext nodeContext)
        {
            // Log all properties before attempting to extract them
            foreach (var prop in activityProperties)
            {
                nodeContext.AddLog($"Property {prop.Key}: {prop.Value} (Type: {prop.Value?.GetType().FullName ?? "null"})");
            }
            
            // Extract properties
            var targetUrl = GetPropertyValue<string>(activityProperties, inputData, "targetUrl");
            var elementSelector = GetPropertyValue<string>(activityProperties, inputData, "elementSelector");

            nodeContext.AddLog($"Extracting text from element '{elementSelector}' on page '{targetUrl}'");

            // Execute the Playwright operation
            var extractedText = await _playwrightService.ExtractTextFromElementAsync(
                targetUrl, elementSelector);

            nodeContext.AddLog($"Extracted text length: {extractedText?.Length ?? 0}");

            // Return the result
            return (ActivityExecutionResult.Completed, new Dictionary<string, object>
            {
                ["extractedText"] = extractedText
            });
        }

        private T GetPropertyValue<T>(
            Dictionary<string, object> activityProperties,
            Dictionary<string, object> inputData,
            string propertyName)
        {
            // Case-insensitive property lookup

            // Try to find the property in input data first
            var inputKey = inputData.Keys
                .FirstOrDefault(k => string.Equals(k, propertyName, StringComparison.OrdinalIgnoreCase));
            
            if (inputKey != null && inputData.TryGetValue(inputKey, out var inputValue))
            {
                if (inputValue is T typedValue)
                {
                    return typedValue;
                }
                
                // Handle JsonElement conversion
                if (inputValue is JsonElement jsonElement)
                {
                    return ConvertJsonElement<T>(jsonElement);
                }
                
                // Try to convert the value
                try
                {
                    return (T)Convert.ChangeType(inputValue, typeof(T));
                }
                catch
                {
                    // Ignore conversion errors and continue checking properties
                }
            }

            // Then check activity properties
            var propKey = activityProperties.Keys
                .FirstOrDefault(k => string.Equals(k, propertyName, StringComparison.OrdinalIgnoreCase));
            
            if (propKey != null && activityProperties.TryGetValue(propKey, out var propValue))
            {
                if (propValue is T propTypedValue)
                {
                    return propTypedValue;
                }
                
                // Handle JsonElement conversion
                if (propValue is JsonElement jsonElement)
                {
                    return ConvertJsonElement<T>(jsonElement);
                }
                
                // Try to convert the value
                try
                {
                    return (T)Convert.ChangeType(propValue, typeof(T));
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Property '{propertyName}' found but could not be converted to type {typeof(T).Name}. Error: {ex.Message}");
                }
            }

            // If not found, throw an error
            throw new InvalidOperationException($"Property '{propertyName}' not found in input data or activity properties");
        }

        private T ConvertJsonElement<T>(JsonElement element)
        {
            Type targetType = typeof(T);
            
            if (targetType == typeof(string))
            {
                return (T)(object)element.GetString();
            }
            else if (targetType == typeof(int))
            {
                return (T)(object)element.GetInt32();
            }
            else if (targetType == typeof(long))
            {
                return (T)(object)element.GetInt64();
            }
            else if (targetType == typeof(double))
            {
                return (T)(object)element.GetDouble();
            }
            else if (targetType == typeof(bool))
            {
                return (T)(object)element.GetBoolean();
            }
            else if (targetType == typeof(DateTime))
            {
                return (T)(object)element.GetDateTime();
            }
            else
            {
                // For more complex types, use JsonSerializer
                return JsonSerializer.Deserialize<T>(element.GetRawText());
            }
        }
    }
}