using Microsoft.Extensions.Logging;
using Modules.Workflow.Activities;
using Modules.Workflow.Domain.Activities;
using Modules.Workflow.Domain.Core;
using Modules.Workflow.Domain.Enums;
using Modules.Workflow.Infrastructure.Services.Workflow.Activities.Data;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Modules.Workflow.Infrastructure.Services.Activities
{
    public class TransformDataActivityExecutor : BaseActivityExecutor
    {
        private readonly TransformDataActivityDescriptor _transformDataDescriptor = new();

        public TransformDataActivityExecutor(ILogger<TransformDataActivityExecutor> logger) 
            : base(logger)
        {
        }

        public override bool CanExecute(string activityType)
        {
            return activityType.StartsWith("ZenFlow.Activities.Data.");
        }

        public override async Task<(ActivityExecutionResult Result, Dictionary<string, object> OutputData)> ExecuteAsync(
            string activityType, 
            Dictionary<string, object> activityProperties, 
            Dictionary<string, object> inputData, 
            WorkflowExecutionContext workflowContext, 
            NodeExecutionContext nodeContext, 
            CancellationToken cancellationToken = default)
        {
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                nodeContext.AddLog($"Executing Data activity: {activityType}");

                switch (activityType)
                {
                    case "ZenFlow.Activities.Data.TransformDataActivity":
                        return await ExecuteTransformDataAsync(
                            activityProperties, inputData, nodeContext);

                    default:
                        throw new NotSupportedException($"Unsupported activity type: {activityType}");
                }
            }, nodeContext);
        }

        private async Task<(ActivityExecutionResult, Dictionary<string, object>)> ExecuteTransformDataAsync(
            Dictionary<string, object> activityProperties,
            Dictionary<string, object> inputData,
            NodeExecutionContext nodeContext)
        {
            try
            {
                // Create a combined dictionary for validation
                var combinedData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                
                // First add activity properties
                foreach (var kvp in activityProperties)
                {
                    combinedData[kvp.Key] = kvp.Value;
                }
                
                // Then add input data (which can override activity properties)
                foreach (var kvp in inputData)
                {
                    combinedData[kvp.Key] = kvp.Value;
                }
                
                // Validate inputs
                var validationErrors = ValidateInputs("ZenFlow.Activities.Data.TransformDataActivity", combinedData);
                if (validationErrors.Any())
                {
                    var errorMessage = string.Join(", ", validationErrors);
                    nodeContext.AddLog($"Validation failed: {errorMessage}");
                    
                    return (ActivityExecutionResult.Failed, new Dictionary<string, object>
                    {
                        ["success"] = false,
                        ["error"] = new Dictionary<string, object>
                        {
                            ["code"] = "VALIDATION_ERROR",
                            ["message"] = "Transform data validation failed",
                            ["details"] = errorMessage
                        }
                    });
                }
                
                // Extract required properties
                var transformationType = GetPropertyValue<string>(activityProperties, inputData, "transformationType");
                var inputProperty = GetPropertyValue<string>(activityProperties, inputData, "inputProperty");
                var outputProperty = GetPropertyValue<string>(activityProperties, inputData, "outputProperty");
                
                nodeContext.AddLog($"Transformation type: {transformationType}, Input: {inputProperty}, Output: {outputProperty}");

                // Get the input data to transform
                var dataToTransform = GetPropertyValue<object>(activityProperties, inputData, inputProperty);
                
                object transformedData;
                
                // Perform transformation based on type
                switch (transformationType.ToLowerInvariant())
                {
                    case "json":
                        transformedData = await ExecuteJsonPathTransformationAsync(
                            dataToTransform, activityProperties, inputData, nodeContext);
                        break;
                        
                    case "regex":
                        transformedData = await ExecuteRegexTransformationAsync(
                            dataToTransform, activityProperties, inputData, nodeContext);
                        break;
                        
                    case "text":
                        transformedData = await ExecuteTextTransformationAsync(
                            dataToTransform, activityProperties, inputData, nodeContext);
                        break;
                        
                    case "format":
                        transformedData = await ExecuteStringFormattingAsync(
                            dataToTransform, activityProperties, inputData, nodeContext);
                        break;
                        
                    default:
                        throw new NotSupportedException($"Unsupported transformation type: {transformationType}");
                }
                
                nodeContext.AddLog($"Transformation completed successfully");
                
                // Return the result with the specified output property
                var outputData = new Dictionary<string, object>
                {
                    ["success"] = true,
                    [outputProperty] = transformedData,
                    ["transformedData"] = transformedData // Also include in standard property
                };
                
                return (ActivityExecutionResult.Completed, outputData);
            }
            catch (Exception ex)
            {
                var error = ex.ToActivityError();
                nodeContext.AddLog($"Failed to transform data: {error.Message}");
                
                return (ActivityExecutionResult.Failed, 
                        new Dictionary<string, object>
                        {
                            ["success"] = false,
                            ["error"] = new Dictionary<string, object>
                            {
                                ["code"] = error.Code,
                                ["message"] = error.Message,
                                ["details"] = error.Details ?? string.Empty
                            }
                        });
            }
        }

        private async Task<object> ExecuteJsonPathTransformationAsync(
            object inputData,
            Dictionary<string, object> activityProperties,
            Dictionary<string, object> inputDataDict,
            NodeExecutionContext nodeContext)
        {
            var jsonPath = GetPropertyValue<string>(activityProperties, inputDataDict, "jsonPath");
            nodeContext.AddLog($"Applying JSONPath: {jsonPath}");
            
            // Convert input data to JSON string if it's not already
            string jsonString;
            if (inputData is string str)
            {
                jsonString = str;
            }
            else
            {
                jsonString = JsonSerializer.Serialize(inputData);
            }
            
            try
            {
                // Parse JSON document
                using var document = JsonDocument.Parse(jsonString);
                var root = document.RootElement;
                
                // Simple JSONPath implementation for basic paths
                var result = ApplySimpleJsonPath(root, jsonPath);
                
                nodeContext.AddLog($"JSONPath extraction completed, found {(result is object[] arr ? arr.Length : 1)} result(s)");
                
                return result;
            }
            catch (Exception ex)
            {
                nodeContext.AddLog($"JSONPath extraction failed: {ex.Message}");
                throw;
            }
        }

        private async Task<object> ExecuteRegexTransformationAsync(
            object inputData,
            Dictionary<string, object> activityProperties,
            Dictionary<string, object> inputDataDict,
            NodeExecutionContext nodeContext)
        {
            var regexPattern = GetPropertyValue<string>(activityProperties, inputDataDict, "regexPattern");
            var replaceValue = GetPropertyValue<string>(activityProperties, inputDataDict, "replaceValue");
            
            nodeContext.AddLog($"Applying regex pattern: {regexPattern}, replacement: {replaceValue}");
            
            var inputString = inputData?.ToString() ?? string.Empty;
            
            // Apply regex replacement
            var regex = new Regex(regexPattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
            var result = regex.Replace(inputString, replaceValue);
            
            nodeContext.AddLog($"Regex transformation completed. Original length: {inputString.Length}, Result length: {result.Length}");
            
            return result;
        }

        private async Task<object> ExecuteTextTransformationAsync(
            object inputData,
            Dictionary<string, object> activityProperties,
            Dictionary<string, object> inputDataDict,
            NodeExecutionContext nodeContext)
        {
            var transformationExpression = GetPropertyValue<string>(activityProperties, inputDataDict, "transformationExpression");
            
            nodeContext.AddLog($"Applying text transformation expression: {transformationExpression}");
            
            var inputString = inputData?.ToString() ?? string.Empty;
            
            // Simple text transformations - this is a basic implementation
            // In a production system, you might want to use a more sophisticated expression evaluator
            var result = ApplySimpleTextTransformation(inputString, transformationExpression);
            
            nodeContext.AddLog($"Text transformation completed");
            
            return result;
        }

        private async Task<object> ExecuteStringFormattingAsync(
            object inputData,
            Dictionary<string, object> activityProperties,
            Dictionary<string, object> inputDataDict,
            NodeExecutionContext nodeContext)
        {
            var transformationExpression = GetPropertyValue<string>(activityProperties, inputDataDict, "transformationExpression");
            
            nodeContext.AddLog($"Applying string formatting template: {transformationExpression}");
            
            // Create a dictionary with all available data for template replacement
            var templateData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            
            // Add all input data
            foreach (var kvp in inputDataDict)
            {
                templateData[kvp.Key] = kvp.Value;
            }
            
            // Add the specific input data with a generic name
            templateData["input"] = inputData;
            templateData["data"] = inputData;
            
            // Simple template replacement using placeholders like {propertyName}
            var result = ApplyStringTemplate(transformationExpression, templateData);
            
            nodeContext.AddLog($"String formatting completed");
            
            return result;
        }

        private object ApplySimpleJsonPath(JsonElement root, string jsonPath)
        {
            // Basic JSONPath implementation for common patterns
            // This supports: $.property, $.array[*], $.array[index], $.object.property
            
            if (string.IsNullOrWhiteSpace(jsonPath) || !jsonPath.StartsWith("$"))
            {
                throw new ArgumentException("JSONPath must start with '$'");
            }
            
            var current = root;
            var path = jsonPath.Substring(1); // Remove the '$'
            
            if (string.IsNullOrEmpty(path) || path == ".")
            {
                return ConvertJsonElementToObject(current);
            }
            
            if (path.StartsWith("."))
            {
                path = path.Substring(1); // Remove the leading dot
            }
            
            var segments = path.Split('.');
            var results = new List<object>();
            
            try
            {
                ProcessJsonPathSegments(current, segments, 0, results);
                
                if (results.Count == 0)
                {
                    return null;
                }
                else if (results.Count == 1)
                {
                    return results[0];
                }
                else
                {
                    return results.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to apply JSONPath '{jsonPath}': {ex.Message}", ex);
            }
        }
        
        private void ProcessJsonPathSegments(JsonElement current, string[] segments, int segmentIndex, List<object> results)
        {
            if (segmentIndex >= segments.Length)
            {
                results.Add(ConvertJsonElementToObject(current));
                return;
            }
            
            var segment = segments[segmentIndex];
            
            // Handle array access like "array[*]" or "array[0]"
            if (segment.Contains("["))
            {
                var bracketIndex = segment.IndexOf('[');
                var propertyName = segment.Substring(0, bracketIndex);
                var indexPart = segment.Substring(bracketIndex + 1, segment.Length - bracketIndex - 2); // Remove [ and ]
                
                // Navigate to the array property first
                if (!string.IsNullOrEmpty(propertyName) && current.ValueKind == JsonValueKind.Object)
                {
                    if (current.TryGetProperty(propertyName, out var arrayElement))
                    {
                        current = arrayElement;
                    }
                    else
                    {
                        return; // Property not found
                    }
                }
                
                if (current.ValueKind == JsonValueKind.Array)
                {
                    if (indexPart == "*")
                    {
                        // Get all elements
                        foreach (var item in current.EnumerateArray())
                        {
                            ProcessJsonPathSegments(item, segments, segmentIndex + 1, results);
                        }
                    }
                    else if (int.TryParse(indexPart, out var index))
                    {
                        // Get specific index
                        var arrayEnum = current.EnumerateArray();
                        var currentIndex = 0;
                        foreach (var item in arrayEnum)
                        {
                            if (currentIndex == index)
                            {
                                ProcessJsonPathSegments(item, segments, segmentIndex + 1, results);
                                break;
                            }
                            currentIndex++;
                        }
                    }
                }
            }
            else
            {
                // Regular property access
                if (current.ValueKind == JsonValueKind.Object && current.TryGetProperty(segment, out var property))
                {
                    ProcessJsonPathSegments(property, segments, segmentIndex + 1, results);
                }
            }
        }
        
        private object ConvertJsonElementToObject(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.TryGetInt32(out var intVal) ? intVal : element.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                JsonValueKind.Array => element.EnumerateArray().Select(ConvertJsonElementToObject).ToArray(),
                JsonValueKind.Object => element.EnumerateObject().ToDictionary(prop => prop.Name, prop => ConvertJsonElementToObject(prop.Value)),
                _ => element.GetRawText()
            };
        }

        private string ApplySimpleTextTransformation(string input, string expression)
        {
            // Basic text transformations - replace with more sophisticated logic as needed
            var result = input;
            
            // Handle common transformations
            if (expression.Contains("toUpperCase()") || expression.Contains("ToUpper()"))
            {
                result = result.ToUpperInvariant();
            }
            else if (expression.Contains("toLowerCase()") || expression.Contains("ToLower()"))
            {
                result = result.ToLowerInvariant();
            }
            
            if (expression.Contains("trim()") || expression.Contains("Trim()"))
            {
                result = result.Trim();
            }
            
            // Handle replace operations in expressions like "input.replace('old', 'new')"
            var replaceMatch = Regex.Match(expression, @"replace\s*\(\s*['""]([^'""]*)['""],\s*['""]([^'""]*)['""]");
            if (replaceMatch.Success)
            {
                var oldValue = replaceMatch.Groups[1].Value;
                var newValue = replaceMatch.Groups[2].Value;
                result = result.Replace(oldValue, newValue);
            }
            
            return result;
        }

        private string ApplyStringTemplate(string template, Dictionary<string, object> data)
        {
            var result = template;
            
            // Replace placeholders like {propertyName} with actual values
            foreach (var kvp in data)
            {
                var placeholder = $"{{{kvp.Key}}}";
                var value = kvp.Value?.ToString() ?? string.Empty;
                result = result.Replace(placeholder, value);
            }
            
            // Handle escape sequences
            result = result.Replace("\\n", "\n");
            result = result.Replace("\\t", "\t");
            result = result.Replace("\\r", "\r");
            
            return result;
        }

        protected override IEnumerable<string> ValidateInputs(string activityType, Dictionary<string, object> inputData)
        {
            var errors = new List<string>();
            
            if (activityType == "ZenFlow.Activities.Data.TransformDataActivity")
            {
                // Validate transformation type
                if (!inputData.TryGetValue("transformationType", out var transformationType) || 
                    transformationType == null || string.IsNullOrWhiteSpace(transformationType.ToString()))
                {
                    errors.Add("transformationType is required");
                }
                else
                {
                    var transformationTypeStr = transformationType.ToString().ToLowerInvariant();
                    var validTypes = new[] { "json", "regex", "text", "format" };
                    
                    if (!validTypes.Contains(transformationTypeStr))
                    {
                        errors.Add($"transformationType must be one of: {string.Join(", ", validTypes)}");
                    }
                    
                    // Validate type-specific required properties
                    switch (transformationTypeStr)
                    {
                        case "json":
                            if (!inputData.ContainsKey("jsonPath") || string.IsNullOrWhiteSpace(inputData["jsonPath"]?.ToString()))
                            {
                                errors.Add("jsonPath is required for 'json' transformation type");
                            }
                            break;
                            
                        case "regex":
                            if (!inputData.ContainsKey("regexPattern") || string.IsNullOrWhiteSpace(inputData["regexPattern"]?.ToString()))
                            {
                                errors.Add("regexPattern is required for 'regex' transformation type");
                            }
                            if (!inputData.ContainsKey("replaceValue"))
                            {
                                errors.Add("replaceValue is required for 'regex' transformation type");
                            }
                            break;
                            
                        case "text":
                        case "format":
                            if (!inputData.ContainsKey("transformationExpression") || string.IsNullOrWhiteSpace(inputData["transformationExpression"]?.ToString()))
                            {
                                errors.Add($"transformationExpression is required for '{transformationTypeStr}' transformation type");
                            }
                            break;
                    }
                }
                
                // Validate required base properties
                if (!inputData.ContainsKey("inputProperty") || string.IsNullOrWhiteSpace(inputData["inputProperty"]?.ToString()))
                {
                    errors.Add("inputProperty is required");
                }
                
                if (!inputData.ContainsKey("outputProperty") || string.IsNullOrWhiteSpace(inputData["outputProperty"]?.ToString()))
                {
                    errors.Add("outputProperty is required");
                }
                
                // Log validation for debugging
                if (errors.Any())
                {
                    _logger.LogWarning("Transform data activity validation failed with errors: {Errors}", 
                        string.Join("; ", errors));
                }
            }
            
            return errors;
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
                if (inputValue is JsonElement jsonElement)
                {
                    return ConvertJsonElement<T>(jsonElement);
                }
                
                if (inputValue is T directValue)
                    return directValue;
                    
                // Try to convert the value
                try
                {
                    return (T)Convert.ChangeType(inputValue, typeof(T));
                }
                catch
                {
                    // Fall through to check activity properties
                }
            }

            // Try to find the property in activity properties
            var propKey = activityProperties.Keys
                .FirstOrDefault(k => string.Equals(k, propertyName, StringComparison.OrdinalIgnoreCase));
                
            if (propKey != null && activityProperties.TryGetValue(propKey, out var propValue))
            {
                if (propValue is JsonElement jsonElement)
                {
                    return ConvertJsonElement<T>(jsonElement);
                }
                
                if (propValue is T directValue)
                    return directValue;
                    
                // Try to convert the value
                try
                {
                    return (T)Convert.ChangeType(propValue, typeof(T));
                }
                catch
                {
                    // If conversion fails, throw an error
                }
            }

            // If not found, throw an error
            throw new InvalidOperationException($"Property '{propertyName}' not found in input data or activity properties");
        }

        private T ConvertJsonElement<T>(JsonElement element)
        {
            if (typeof(T) == typeof(string))
                return (T)(object)element.GetString();
            if (typeof(T) == typeof(bool))
                return (T)(object)element.GetBoolean();
            if (typeof(T) == typeof(int))
                return (T)(object)element.GetInt32();
            if (typeof(T) == typeof(long))
                return (T)(object)element.GetInt64();
            if (typeof(T) == typeof(double))
                return (T)(object)element.GetDouble();
            if (typeof(T) == typeof(object))
                return (T)(object)System.Text.Json.JsonSerializer.Deserialize<object>(element.GetRawText());

            throw new InvalidOperationException($"Cannot convert JsonElement to type {typeof(T).Name}");
        }
    }
} 