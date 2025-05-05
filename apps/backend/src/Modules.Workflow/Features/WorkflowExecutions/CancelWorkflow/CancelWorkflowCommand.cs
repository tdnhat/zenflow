using System;
using MediatR;

namespace Modules.Workflow.Features.WorkflowExecutions.CancelWorkflow
{
    public record CancelWorkflowCommand : IRequest<CancelWorkflowResult>
    {
        /// <summary>
        /// The ID of the workflow to cancel
        /// </summary>
        public string? WorkflowId { get; init; }
        
        /// <summary>
        /// The ID of the specific execution to cancel (if multiple executions exist)
        /// </summary>
        public string? ExecutionId { get; init; }
        
        /// <summary>
        /// Optional reason for cancellation
        /// </summary>
        public string? Reason { get; init; }
    }
}
