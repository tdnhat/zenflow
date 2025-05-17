using Microsoft.Extensions.Logging;
using Modules.Workflow.Activities;
using Modules.Workflow.Domain.Core;
using Modules.Workflow.Domain.Enums;
using Modules.Workflow.Domain.Interfaces.Core;

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
            var targetUrl = GetPropertyValue<string>(activityProperties, inputData, "TargetUrl");
            var elementSelector = GetPropertyValue<string>(activityProperties, inputData, "ElementSelector");
            var attributeName = GetPropertyValue<string>(activityProperties, inputData, "AttributeName");

            nodeContext.AddLog($"Getting attribute '{attributeName}' from element '{elementSelector}' on page '{targetUrl}'");

            // Execute the Playwright operation
            var attributeValue = await _playwrightService.GetElementAttributeAsync(
                targetUrl, elementSelector, attributeName);

            nodeContext.AddLog($"Attribute value: {attributeValue}");

            // Return the result
            return (ActivityExecutionResult.Completed, new Dictionary<string, object>
            {
                ["AttributeValue"] = attributeValue
            });
        }

        private async Task<(ActivityExecutionResult, Dictionary<string, object>)> ExecuteExtractTextFromElementAsync(
            Dictionary<string, object> activityProperties,
            Dictionary<string, object> inputData,
            NodeExecutionContext nodeContext)
        {
            // Extract properties
            var targetUrl = GetPropertyValue<string>(activityProperties, inputData, "TargetUrl");
            var elementSelector = GetPropertyValue<string>(activityProperties, inputData, "ElementSelector");

            nodeContext.AddLog($"Extracting text from element '{elementSelector}' on page '{targetUrl}'");

            // Execute the Playwright operation
            var extractedText = await _playwrightService.ExtractTextFromElementAsync(
                targetUrl, elementSelector);

            nodeContext.AddLog($"Extracted text length: {extractedText?.Length ?? 0}");

            // Return the result
            return (ActivityExecutionResult.Completed, new Dictionary<string, object>
            {
                ["ExtractedText"] = extractedText
            });
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