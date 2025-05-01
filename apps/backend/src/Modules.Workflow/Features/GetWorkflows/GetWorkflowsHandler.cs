using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Interfaces;
using Modules.Workflow.Dtos;
using ZenFlow.Shared.Application.Auth;
using ZenFlow.Shared.Application.Models;
using System.Linq;

namespace Modules.Workflow.Features.GetWorkflows
{
    public class GetWorkflowsHandler : IRequestHandler<GetWorkflowsQuery, PaginatedResult<WorkflowDto>>
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

        public async Task<PaginatedResult<WorkflowDto>> Handle(GetWorkflowsQuery request, CancellationToken cancellationToken)
        {
            // Get the current user's ID
            var userId = _currentUser.UserId;
            
            _logger.LogInformation("Retrieving filtered workflows for user {UserId} with page {Page} and pageSize {PageSize}", 
                userId, request.Filter.Page, request.Filter.PageSize);
            
            // Get filtered and paginated workflows
            var paginatedWorkflows = await _workflowRepository.GetFilteredAsync(userId, request.Filter, cancellationToken);
            
            // Map entity results to DTOs
            var workflowDtos = paginatedWorkflows.Items.Select(w => 
                new WorkflowDto(w.Id, w.Name, w.Description, w.Status)).ToList();
            
            _logger.LogInformation("Retrieved {Count} workflows for user {UserId} (Total: {Total})", 
                workflowDtos.Count, userId, paginatedWorkflows.TotalCount);
            
            // Create a new paginated result with the mapped DTOs
            return new PaginatedResult<WorkflowDto>(
                workflowDtos,
                paginatedWorkflows.TotalCount,
                paginatedWorkflows.Page,
                paginatedWorkflows.PageSize);
        }
    }
}