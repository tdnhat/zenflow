using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Workflow.Domain.Interfaces.Core;

namespace Modules.Workflow.Features.Workflows.DeleteWorkflowDefinition
{
    public class DeleteWorkflowDefinitionHandler : IRequestHandler<DeleteWorkflowDefinitionCommand>
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly ILogger<DeleteWorkflowDefinitionHandler> _logger;

        public DeleteWorkflowDefinitionHandler(
            IWorkflowRepository workflowRepository,
            ILogger<DeleteWorkflowDefinitionHandler> logger)
        {
            _workflowRepository = workflowRepository;
            _logger = logger;
        }

        public async Task Handle(DeleteWorkflowDefinitionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting workflow definition with ID {WorkflowId}", request.WorkflowId);

            // Check if workflow exists
            var existingWorkflow = await _workflowRepository.GetByIdAsync(request.WorkflowId, cancellationToken);
            if (existingWorkflow == null)
            {
                _logger.LogWarning("Workflow definition with ID {WorkflowId} not found", request.WorkflowId);
                throw new KeyNotFoundException($"Workflow definition with ID {request.WorkflowId} not found");
            }

            // Delete from repository
            await _workflowRepository.DeleteAsync(request.WorkflowId, cancellationToken);

            _logger.LogInformation("Successfully deleted workflow definition with ID {WorkflowId}", request.WorkflowId);
        }
    }
}