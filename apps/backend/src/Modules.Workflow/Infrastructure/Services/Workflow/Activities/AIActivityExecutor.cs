using Microsoft.Extensions.Logging;
using Modules.Workflow.Activities;
using Modules.Workflow.Domain.Core;
using Modules.Workflow.Domain.Enums;
using Modules.Workflow.Domain.Interfaces.Core;

namespace Modules.Workflow.Infrastructure.Services.Activities
{
    public class AIActivityExecutor : BaseActivityExecutor
    {
        private readonly IAIService _aiService;

        public AIActivityExecutor(ILogger<AIActivityExecutor> logger, IAIService aiService) 
            : base(logger)
        {
            _aiService = aiService;
        }

        public override bool CanExecute(string activityType)
        {
            return activityType.StartsWith("ZenFlow.Activities.AI.");
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
                nodeContext.AddLog($"Executing AI activity: {activityType}");

                switch (activityType)
                {
                    case "ZenFlow.Activities.AI.SummarizeTextActivity":
                        return await ExecuteSummarizeTextAsync(
                            activityProperties, inputData, nodeContext);

                    default:
                        throw new NotSupportedException($"Unsupported activity type: {activityType}");
                }
            }, nodeContext);
        }

        private async Task<(ActivityExecutionResult, Dictionary<string, object>)> ExecuteSummarizeTextAsync(
            Dictionary<string, object> activityProperties,
            Dictionary<string, object> inputData,
            NodeExecutionContext nodeContext)
        {
            // Extract activity properties
            var text = GetPropertyValue<string>(activityProperties, inputData, "TextToSummarize");
            
            nodeContext.AddLog($"Summarizing text of length: {text?.Length ?? 0}");

            // Execute AI summarization
            var summary = await _aiService.SummarizeTextAsync(text ?? string.Empty);

            nodeContext.AddLog($"Summary length: {summary?.Length ?? 0}");

            // Return the result
            return (ActivityExecutionResult.Completed, new Dictionary<string, object>
            {
                ["Summary"] = summary
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