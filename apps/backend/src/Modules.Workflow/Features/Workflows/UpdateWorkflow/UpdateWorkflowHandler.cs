using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Interfaces;
using Modules.Workflow.Dtos;
using ZenFlow.Shared.Application.Auth;

namespace Modules.Workflow.Features.Workflows.UpdateWorkflow
{
    public class UpdateWorkflowHandler : IRequestHandler<UpdateWorkflowCommand, WorkflowDto?>
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<UpdateWorkflowHandler> _logger;

        public UpdateWorkflowHandler(
            IWorkflowRepository workflowRepository,
            ICurrentUserService currentUser,
            ILogger<UpdateWorkflowHandler> logger)
        {
            _workflowRepository = workflowRepository;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<WorkflowDto?> Handle(UpdateWorkflowCommand request, CancellationToken cancellationToken)
        {
            var workflow = await _workflowRepository.GetByIdAsync(request.Id, cancellationToken);

            if (workflow == null)
            {
                _logger.LogWarning("Workflow with ID {WorkflowId} not found", request.Id);
                return null;
            }

            // Security check: ensure the user can only update their own workflows
            if (workflow.CreatedBy != _currentUser.UserId)
            {
                _logger.LogWarning("User {UserId} attempted to update workflow {WorkflowId} which belongs to another user",
                    _currentUser.UserId, request.Id);
                return null;
            }

            // Use the Update method from the entity
            workflow.Update(request.Name, request.Description);

            await _workflowRepository.UpdateAsync(workflow, cancellationToken);
            await _workflowRepository.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Updated workflow {WorkflowId} for user {UserId}", workflow.Id, _currentUser.UserId);

            return new WorkflowDto(workflow.Id, workflow.Name, workflow.Description, workflow.Status.ToString());
        }
    }
}