using Microsoft.Extensions.Logging;
using Modules.Workflow.Activities;
using Modules.Workflow.Domain.Core;
using Modules.Workflow.Domain.Enums;
using Modules.Workflow.Domain.Interfaces.Core;
using System.Linq;
using System.Text.Json;

namespace Modules.Workflow.Infrastructure.Services.Activities
{
    public class EmailActivityExecutor : BaseActivityExecutor
    {
        private readonly IEmailService _emailService;

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
            // Extract activity properties
            var to = GetPropertyValue<string>(activityProperties, inputData, "to");
            var subject = GetPropertyValue<string>(activityProperties, inputData, "subject");
            var body = GetPropertyValue<string>(activityProperties, inputData, "body");
            
            nodeContext.AddLog($"Sending email to {to} with subject: {subject}");

            // Convert single email to IEnumerable<string>
            var toAddresses = new[] { to };

            try
            {
                // Execute email sending
                await _emailService.SendEmailAsync(toAddresses, subject, body);
                
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
                nodeContext.AddLog($"Failed to send email: {ex.Message}");
                
                return (ActivityExecutionResult.Failed, 
                        new Dictionary<string, object>
                        {
                            ["success"] = false
                        });
            }
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