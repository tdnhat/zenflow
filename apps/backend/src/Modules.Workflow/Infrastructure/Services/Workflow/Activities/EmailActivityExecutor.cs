using Microsoft.Extensions.Logging;
using Modules.Workflow.Activities;
using Modules.Workflow.Domain.Activities;
using Modules.Workflow.Domain.Core;
using Modules.Workflow.Domain.Enums;
using Modules.Workflow.Domain.Interfaces.Core;
using Modules.Workflow.Infrastructure.Services.Workflow.Activities.Email;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;

namespace Modules.Workflow.Infrastructure.Services.Activities
{
    public class EmailActivityExecutor : BaseActivityExecutor
    {
        private readonly IEmailService _emailService;
        private readonly SendEmailActivityDescriptor _sendEmailDescriptor = new();

        public EmailActivityExecutor(ILogger<EmailActivityExecutor> logger, IEmailService emailService) 
            : base(logger)
        {
            _emailService = emailService;
        }

        public override bool CanExecute(string activityType)
        {
            return activityType.StartsWith("ZenFlow.Activities.Email.");
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
                nodeContext.AddLog($"Executing Email activity: {activityType}");

                switch (activityType)
                {
                    case "ZenFlow.Activities.Email.SendEmailActivity":
                        return await ExecuteSendEmailAsync(
                            activityProperties, inputData, nodeContext);

                    default:
                        throw new NotSupportedException($"Unsupported activity type: {activityType}");
                }
            }, nodeContext);
        }

        private async Task<(ActivityExecutionResult, Dictionary<string, object>)> ExecuteSendEmailAsync(
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
                var validationErrors = ValidateInputs("ZenFlow.Activities.Email.SendEmailActivity", combinedData);
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
                            ["message"] = "Email validation failed",
                            ["details"] = errorMessage
                        }
                    });
                }
                
                // Extract activity properties
                var to = GetPropertyValue<string>(activityProperties, inputData, "to");
                var subject = GetPropertyValue<string>(activityProperties, inputData, "subject");
                var body = GetPropertyValue<string>(activityProperties, inputData, "body");
                
                // Optional properties
                var isHtml = GetPropertyValueOrDefault<bool>(activityProperties, inputData, "isHtml", false);
                var cc = GetPropertyValueOrDefault<string>(activityProperties, inputData, "cc", null);
                var bcc = GetPropertyValueOrDefault<string>(activityProperties, inputData, "bcc", null);
                
                nodeContext.AddLog($"Sending email to {to} with subject: {subject}");

                // Convert single email to IEnumerable<string>
                var toAddresses = new[] { to };
                
                // Parse CC and BCC if provided
                var ccAddresses = string.IsNullOrWhiteSpace(cc) ? Array.Empty<string>() : 
                    cc.Split(',').Select(e => e.Trim()).Where(e => !string.IsNullOrEmpty(e)).ToArray();
                    
                var bccAddresses = string.IsNullOrWhiteSpace(bcc) ? Array.Empty<string>() : 
                    bcc.Split(',').Select(e => e.Trim()).Where(e => !string.IsNullOrEmpty(e)).ToArray();

                // Execute email sending
                await _emailService.SendEmailAsync(
                    toAddresses, 
                    subject, 
                    body, 
                    isHtml, 
                    ccAddresses, 
                    bccAddresses);
                
                nodeContext.AddLog("Email sent successfully");
                
                // Return the result
                return (ActivityExecutionResult.Completed, 
                        new Dictionary<string, object>
                        {
                            ["success"] = true
                        });
            }
            catch (Exception ex)
            {
                var error = ex.ToActivityError();
                nodeContext.AddLog($"Failed to send email: {error.Message}");
                
                return (ActivityExecutionResult.Failed, 
                        new Dictionary<string, object>
                        {
                            ["success"] = false,
                            ["error"] = new Dictionary<string, object>
                            {
                                ["code"] = error.Code,
                                ["message"] = error.Message
                            }
                        });
            }
        }

        protected override IEnumerable<string> ValidateInputs(string activityType, Dictionary<string, object> inputData)
        {
            var errors = new List<string>();
            
            if (activityType == "ZenFlow.Activities.Email.SendEmailActivity")
            {
                // Validate required properties from the descriptor
                foreach (var prop in _sendEmailDescriptor.InputProperties.Where(p => p.IsRequired))
                {
                    if (!inputData.ContainsKey(prop.Name) || inputData[prop.Name] == null)
                    {
                        errors.Add($"Required property '{prop.Name}' is missing or null");
                    }
                }
                
                // Validate email format if 'to' is present
                if (inputData.TryGetValue("to", out var toValue) && toValue is string to)
                {
                    var emailAttr = new EmailAddressAttribute();
                    if (!emailAttr.IsValid(to))
                    {
                        errors.Add($"Invalid email address format for 'to': {to}");
                    }
                }
                
                // Log the validation for debugging
                if (errors.Any())
                {
                    _logger.LogWarning("Email activity validation failed with errors: {Errors}", 
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
        
        private T GetPropertyValueOrDefault<T>(
            Dictionary<string, object> activityProperties,
            Dictionary<string, object> inputData,
            string propertyName,
            T defaultValue)
        {
            try
            {
                return GetPropertyValue<T>(activityProperties, inputData, propertyName);
            }
            catch
            {
                return defaultValue;
            }
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