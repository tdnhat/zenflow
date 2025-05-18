using Microsoft.Extensions.Logging;
using Modules.Workflow.Activities;
using Modules.Workflow.Domain.Core;
using Modules.Workflow.Domain.Enums;
using Modules.Workflow.Domain.Interfaces.Core;

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
            var to = GetPropertyValue<string>(activityProperties, inputData, "To");
            var subject = GetPropertyValue<string>(activityProperties, inputData, "Subject");
            var body = GetPropertyValue<string>(activityProperties, inputData, "Body");
            
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
                            ["Success"] = true
                        });
            }
            catch (Exception ex)
            {
                nodeContext.AddLog($"Failed to send email: {ex.Message}");
                
                return (ActivityExecutionResult.Failed, 
                        new Dictionary<string, object>
                        {
                            ["Success"] = false
                        });
            }
        }

        private T GetPropertyValue<T>(
            Dictionary<string, object> activityProperties,
            Dictionary<string, object> inputData,
            string propertyName)
        {
            // First check input data (which might come from previous nodes)
            if (inputData.TryGetValue(propertyName, out var value) && value is T typedValue)
                return typedValue;

            // Then check activity properties (from the workflow definition)
            if (activityProperties.TryGetValue(propertyName, out var propValue) && propValue is T propTypedValue)
                return propTypedValue;

            // If not found, throw an error
            throw new InvalidOperationException($"Property '{propertyName}' not found in input data or activity properties");
        }
    }
} 