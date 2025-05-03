using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Interfaces;
using Modules.Workflow.Dtos;
using ZenFlow.Shared.Application.Auth;

namespace Modules.Workflow.Features.Workflows.GetWorkflows
{
    public class GetWorkflowsHandler : IRequestHandler<GetWorkflowsQuery, List<WorkflowDto>>
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

        public async Task<List<WorkflowDto>> Handle(GetWorkflowsQuery request, CancellationToken cancellationToken)
        {
            // Get the current user's ID
            var userId = _currentUser.UserId;

            _logger.LogInformation("Retrieving workflows for user {UserId}", userId);

            // Get all workflows
            var workflows = await _workflowRepository.GetAllAsync(cancellationToken: cancellationToken);

            // Map entity results to DTOs
            var workflowDtos = workflows.Select(w =>
                new WorkflowDto(w.Id, w.Name, w.Description, w.Status.ToString())).ToList();

            _logger.LogInformation("Retrieved {Count} workflows for user {UserId} (Total: {Total})",
                workflowDtos.Count, userId, workflows.Count);

            return workflowDtos;
        }
    }
}