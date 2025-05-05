using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Workflow.Features.WorkflowExecutions.CancelWorkflow
{
    public class CancelWorkflowHandler : IRequestHandler<CancelWorkflowCommand, CancelWorkflowResult>
    {
        private readonly ILogger<CancelWorkflowHandler> _logger;
        private readonly IWorkflowExecutionRepository _executionRepository;
        private readonly IBrowserSessionManager _browserSessionManager;

        public CancelWorkflowHandler(
            ILogger<CancelWorkflowHandler> logger,
            IWorkflowExecutionRepository executionRepository,
            IBrowserSessionManager browserSessionManager)
        {
            _logger = logger;
            _executionRepository = executionRepository;
            _browserSessionManager = browserSessionManager;
        }

        public async Task<CancelWorkflowResult> Handle(CancelWorkflowCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing cancellation request for workflow {WorkflowId}, execution {ExecutionId}", 
                    request.WorkflowId, request.ExecutionId);

                // If an execution ID is provided, use that directly
                if (!string.IsNullOrEmpty(request.ExecutionId) && Guid.TryParse(request.ExecutionId, out var executionId))
                {
                    var execution = await _executionRepository.GetByIdAsync(executionId, cancellationToken);
                    if (execution == null)
                    {
                        _logger.LogWarning("Execution with ID {ExecutionId} not found", request.ExecutionId);
                        return new CancelWorkflowResult
                        {
                            Success = false,
                            Message = $"Execution with ID {request.ExecutionId} not found"
                        };
                    }

                    // Cancel browser sessions associated with this execution
                    await _browserSessionManager.CloseSessionAsync(execution.Id.ToString(), cancellationToken);

                    // Update execution status
                    var reason = !string.IsNullOrEmpty(request.Reason) 
                        ? request.Reason 
                        : "Canceled by user request";
                        
                    execution.Cancel(reason);
                    await _executionRepository.UpdateAsync(execution, cancellationToken);
                    await _executionRepository.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Successfully canceled execution {ExecutionId}", execution.Id);
                    return new CancelWorkflowResult
                    {
                        Success = true,
                        Message = "Workflow execution canceled successfully",
                        ExecutionId = execution.Id.ToString(),
                        Status = execution.Status.ToString()
                    };
                }
                // Otherwise, look up by workflow ID
                else if (!string.IsNullOrEmpty(request.WorkflowId) && Guid.TryParse(request.WorkflowId, out var workflowId))
                {
                    // Get the most recent active execution for this workflow
                    var execution = await _executionRepository.GetMostRecentActiveExecutionForWorkflowAsync(workflowId, cancellationToken);
                    if (execution == null)
                    {
                        _logger.LogWarning("No active executions found for workflow {WorkflowId}", request.WorkflowId);
                        return new CancelWorkflowResult
                        {
                            Success = false,
                            Message = $"No active executions found for workflow {request.WorkflowId}"
                        };
                    }

                    // Cancel browser sessions associated with this execution
                    await _browserSessionManager.CloseSessionAsync(execution.Id.ToString(), cancellationToken);

                    // Update execution status
                    var reason = !string.IsNullOrEmpty(request.Reason) 
                        ? request.Reason 
                        : "Canceled by user request";
                        
                    execution.Cancel(reason);
                    await _executionRepository.UpdateAsync(execution, cancellationToken);
                    await _executionRepository.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Successfully canceled execution {ExecutionId} for workflow {WorkflowId}",
                        execution.Id, request.WorkflowId);
                    return new CancelWorkflowResult
                    {
                        Success = true,
                        Message = "Workflow execution canceled successfully",
                        ExecutionId = execution.Id.ToString(),
                        Status = execution.Status.ToString()
                    };
                }
                else
                {
                    _logger.LogWarning("Invalid request: Either WorkflowId or ExecutionId must be provided");
                    return new CancelWorkflowResult
                    {
                        Success = false,
                        Message = "Either WorkflowId or ExecutionId must be provided"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling workflow {WorkflowId}, execution {ExecutionId}: {ErrorMessage}",
                    request.WorkflowId, request.ExecutionId, ex.Message);

                return new CancelWorkflowResult
                {
                    Success = false,
                    Message = $"Error canceling workflow execution: {ex.Message}"
                };
            }
        }
    }
}
