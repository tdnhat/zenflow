using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Interfaces;
using Modules.Workflow.Dtos;
using ZenFlow.Shared.Application.Auth;

namespace Modules.Workflow.Features.GetWorkflows
{
    public class GetWorkflowsHandler : IRequestHandler<GetWorkflowsQuery, IEnumerable<WorkflowDto>>
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<GetWorkflowsHandler> _logger;

        public GetWorkflowsHandler(
            IWorkflowRepository workflowRepository,
            ICurrentUserService currentUser,
            ILogger<GetWorkflowsHandler> logger)
        {
            _workflowRepository = workflowRepository;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<IEnumerable<WorkflowDto>> Handle(GetWorkflowsQuery request, CancellationToken cancellationToken)
        {
            // Get the current user's ID
            var userId = _currentUser.UserId;
            
            _logger.LogInformation("Retrieving workflows for user {UserId}", userId);

            // Query workflows that belong to the current user using the repository
            var workflows = await _workflowRepository.GetByUserIdAsync(userId, cancellationToken);
            
            // Map entities to DTOs
            var workflowDtos = workflows.Select(w => new WorkflowDto(w.Id, w.Name, w.Description, w.Status)).ToList();

            _logger.LogInformation("Retrieved {Count} workflows for user {UserId}", workflowDtos.Count, userId);
            
            return workflowDtos;
        }
    }
}