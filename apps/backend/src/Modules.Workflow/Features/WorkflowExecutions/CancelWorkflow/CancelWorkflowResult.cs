using Elsa.Models;
using Modules.Workflow.DDD.ValueObjects;

namespace Modules.Workflow.Features.WorkflowExecutions.CancelWorkflow
{
    public class CancelWorkflowResult
    {
        /// <summary>
        /// Whether the cancellation was successful
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// A message describing the result of the cancellation attempt
        /// </summary>
        public string Message { get; set; } = string.Empty;
        
        /// <summary>
        /// The ID of the execution that was canceled
        /// </summary>
        public string? ExecutionId { get; set; }
        
        /// <summary>
        /// The status of the workflow execution after the cancellation attempt
        /// </summary>
        public string? Status { get; set; }
    }
}
