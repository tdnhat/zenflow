using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Interfaces;
using Modules.Workflow.DDD.ValueObjects;

namespace Modules.Workflow.Features.WorkflowExecutions.CancelWorkflow
{
    public class CancelWorkflowHandler : IRequestHandler<CancelWorkflowCommand, CancelWorkflowResult>
    {
        private readonly IWorkflowExecutionRepository _executionRepository;
        private readonly ILogger<CancelWorkflowHandler> _logger;

        public CancelWorkflowHandler(IWorkflowExecutionRepository executionRepository, ILogger<CancelWorkflowHandler> logger)
        {
            _executionRepository = executionRepository;
            _logger = logger;
        }

        public async Task<CancelWorkflowResult> Handle(CancelWorkflowCommand request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(request.WorkflowId, out var workflowIdGuid))
            {
                _logger.LogWarning("Invalid Workflow ID format: {WorkflowId}", request.WorkflowId);
                return new CancelWorkflowResult { Success = false, Message = "Invalid Workflow ID format." };
            }

            try
            {
                _logger.LogInformation("Attempting to cancel latest execution for workflow ID {WorkflowId}", workflowIdGuid);

                // Find the latest execution for the workflow ID that might be running or pending
                var executions = await _executionRepository.GetByWorkflowIdAsync(workflowIdGuid, 0, 1, cancellationToken);
                var latestExecution = executions.FirstOrDefault();

                if (latestExecution == null)
                {
                    _logger.LogWarning("No executions found for workflow ID {WorkflowId} to cancel.", workflowIdGuid);
                    return new CancelWorkflowResult { Success = false, Message = $"No executions found for workflow ID {request.WorkflowId}." };
                }

                // Check if the execution is in a cancellable state
                if (latestExecution.Status == WorkflowExecutionStatus.RUNNING || latestExecution.Status == WorkflowExecutionStatus.PENDING)
                {
                    try
                    {
                        latestExecution.Cancel();
                        await _executionRepository.UpdateAsync(latestExecution, cancellationToken);
                        await _executionRepository.SaveChangesAsync(cancellationToken);

                        _logger.LogInformation("Successfully cancelled workflow execution {ExecutionId} for workflow {WorkflowId}", latestExecution.Id, workflowIdGuid);
                        return new CancelWorkflowResult
                        {
                            Success = true,
                            Message = "Workflow execution cancellation requested.",
                            ExecutionId = latestExecution.Id.ToString(),
                            Status = latestExecution.Status
                        };
                    }
                    catch (InvalidOperationException ex)
                    {
                        _logger.LogWarning("Cannot cancel workflow execution {ExecutionId} in status {Status}: {Message}", latestExecution.Id, latestExecution.Status, ex.Message);
                        return new CancelWorkflowResult { Success = false, Message = ex.Message, ExecutionId = latestExecution.Id.ToString(), Status = latestExecution.Status };
                    }
                }
                else
                {
                    _logger.LogInformation("Workflow execution {ExecutionId} is already in a terminal state ({Status}) and cannot be cancelled.", latestExecution.Id, latestExecution.Status);
                    return new CancelWorkflowResult
                    {
                        Success = false, // Or true, depending on whether attempting to cancel a finished workflow is an "error"
                        Message = $"Workflow execution is already in status '{latestExecution.Status}' and cannot be cancelled.",
                        ExecutionId = latestExecution.Id.ToString(),
                        Status = latestExecution.Status
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling workflow execution for workflow ID {WorkflowId}: {ErrorMessage}", workflowIdGuid, ex.Message);
                return new CancelWorkflowResult
                {
                    Success = false,
                    Message = $"An error occurred while cancelling the workflow execution: {ex.Message}"
                };
            }
        }
    }
}
