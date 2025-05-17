using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Workflow.Domain.Interfaces.Core;

namespace Modules.Workflow.Features.Workflows.CancelWorkflowRun
{
    public class CancelWorkflowRunHandler : IRequestHandler<CancelWorkflowRunCommand>
    {
        private readonly IWorkflowEngine _workflowEngine;
        private readonly ILogger<CancelWorkflowRunHandler> _logger;

        public CancelWorkflowRunHandler(
            IWorkflowEngine workflowEngine,
            ILogger<CancelWorkflowRunHandler> logger)
        {
            _workflowEngine = workflowEngine;
            _logger = logger;
        }        public async Task Handle(CancelWorkflowRunCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Cancelling workflow run with ID {WorkflowRunId}", request.WorkflowRunId);
            
            try
            {
                // Cancel the workflow execution
                await _workflowEngine.CancelWorkflowAsync(request.WorkflowRunId, cancellationToken);
                
                _logger.LogInformation("Successfully cancelled workflow run with ID {WorkflowRunId}", 
                    request.WorkflowRunId);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Workflow run with ID {WorkflowRunId} not found", request.WorkflowRunId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling workflow run with ID {WorkflowRunId}", request.WorkflowRunId);
                throw;
            }
        }
    }
}