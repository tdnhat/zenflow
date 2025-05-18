using Microsoft.Extensions.Logging;
using Modules.Workflow.Domain.Activities;
using Modules.Workflow.Domain.Core;
using Modules.Workflow.Domain.Enums;
using Modules.Workflow.Domain.Interfaces.Core;
using System.ComponentModel.DataAnnotations;

namespace Modules.Workflow.Activities
{
    public abstract class BaseActivityExecutor : IActivityExecutor
    {
        protected readonly ILogger _logger;

        public BaseActivityExecutor(ILogger logger)
        {
            _logger = logger;
        }

        public abstract bool CanExecute(string activityType);

        public abstract Task<(ActivityExecutionResult Result, Dictionary<string, object> OutputData)> ExecuteAsync(
            string activityType,
            Dictionary<string, object> activityProperties,
            Dictionary<string, object> inputData,
            WorkflowExecutionContext workflowContext,
            NodeExecutionContext nodeContext,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Execute with error handling
        /// </summary>
        protected async Task<(ActivityExecutionResult Result, Dictionary<string, object> OutputData)> ExecuteWithErrorHandlingAsync(
            Func<Task<(ActivityExecutionResult, Dictionary<string, object>)>> execution,
            NodeExecutionContext nodeContext)
        {
            try
            {
                // Validate inputs before execution
                // We can't validate nodeContext.InputData directly because it might be empty
                // Instead, we defer validation to the concrete activity executors
                
                // Execute the activity
                return await execution();
            }
            catch (ValidationException vex)
            {
                var error = ActivityError.ValidationError(vex.Message, vex.ValidationAttribute?.GetType().Name);
                return HandleActivityError(error, nodeContext);
            }
            catch (Exception ex)
            {
                var error = ex.ToActivityError();
                return HandleActivityError(error, nodeContext);
            }
        }
        
        /// <summary>
        /// Handle an activity error
        /// </summary>
        protected (ActivityExecutionResult, Dictionary<string, object>) HandleActivityError(
            ActivityError error, 
            NodeExecutionContext nodeContext)
        {
            _logger.LogError("Error executing activity {ActivityType}: {ErrorCode} - {ErrorMessage}",
                nodeContext.ActivityType, error.Code, error.Message);

            nodeContext.Error = error.Message;
            nodeContext.AddLog($"Error ({error.Code}): {error.Message}");
            
            if (!string.IsNullOrEmpty(error.Details))
            {
                nodeContext.AddLog($"Details: {error.Details}");
            }

            return (ActivityExecutionResult.Failed, new Dictionary<string, object>
            {
                ["error"] = new Dictionary<string, object>
                {
                    ["code"] = error.Code,
                    ["message"] = error.Message,
                    ["details"] = error.Details ?? string.Empty,
                    ["timestamp"] = error.Timestamp
                }
            });
        }
        
        /// <summary>
        /// Validate activity inputs
        /// </summary>
        protected virtual IEnumerable<string> ValidateInputs(string activityType, Dictionary<string, object> inputData)
        {
            // Base implementation - no validation by default
            // Derived classes should override to provide specific validation
            return Enumerable.Empty<string>();
        }
    }
}