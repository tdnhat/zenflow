using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Interfaces;
using Modules.Workflow.Dtos;
using ZenFlow.Shared.Application.Auth;

namespace Modules.Workflow.Features.RestoreWorkflow
{
    public class RestoreWorkflowHandler : IRequestHandler<RestoreWorkflowCommand, WorkflowDto?>
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<RestoreWorkflowHandler> _logger;

        public RestoreWorkflowHandler(
            IWorkflowRepository workflowRepository,
            ICurrentUserService currentUser,
            ILogger<RestoreWorkflowHandler> logger)
        {
            _workflowRepository = workflowRepository;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<WorkflowDto?> Handle(RestoreWorkflowCommand request, CancellationToken cancellationToken)
        {
            var workflow = await _workflowRepository.GetByIdAsync(request.Id, cancellationToken);

            if (workflow == null)
            {
                _logger.LogWarning("Workflow with ID {WorkflowId} not found", request.Id);
                return null;
            }

            // Security check: ensure the user can only restore their own workflows
            if (workflow.CreatedBy != _currentUser.UserId)
            {
                _logger.LogWarning("User {UserId} attempted to restore workflow {WorkflowId} which belongs to another user", 
                    _currentUser.UserId, request.Id);
                return null;
            }

            // Use the Restore method from the entity
            workflow.Restore();
            
            await _workflowRepository.UpdateAsync(workflow, cancellationToken);

            _logger.LogInformation("Restored workflow {WorkflowId} for user {UserId}", workflow.Id, _currentUser.UserId);
            
            return new WorkflowDto(workflow.Id, workflow.Name, workflow.Description, workflow.Status);
        }
    }
}