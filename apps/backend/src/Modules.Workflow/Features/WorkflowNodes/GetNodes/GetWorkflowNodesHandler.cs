using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Interfaces;
using Modules.Workflow.Dtos;
using System.Linq;
using ZenFlow.Shared.Application.Auth;

namespace Modules.Workflow.Features.WorkflowNodes.GetNodes
{
    public class GetWorkflowNodesHandler : IRequestHandler<GetWorkflowNodesQuery, IEnumerable<WorkflowNodeDto>?>
    {
        private readonly IWorkflowNodeRepository _nodeRepository;
        private readonly IWorkflowRepository _workflowRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<GetWorkflowNodesHandler> _logger;

        public GetWorkflowNodesHandler(
            IWorkflowNodeRepository nodeRepository,
            IWorkflowRepository workflowRepository,
            ICurrentUserService currentUser,
            ILogger<GetWorkflowNodesHandler> logger)
        {
            _nodeRepository = nodeRepository;
            _workflowRepository = workflowRepository;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<IEnumerable<WorkflowNodeDto>?> Handle(GetWorkflowNodesQuery request, CancellationToken cancellationToken)
        {
            // Verify the workflow exists and belongs to the current user
            var workflow = await _workflowRepository.GetByIdAsync(request.WorkflowId, cancellationToken);
            
            if (workflow == null)
            {
                _logger.LogWarning("Workflow with ID {WorkflowId} not found", request.WorkflowId);
                return null;
            }

            // Security check: ensure the user can only access their own workflows
            if (workflow.CreatedBy != _currentUser.UserId)
            {
                _logger.LogWarning("User {UserId} attempted to access nodes for workflow {WorkflowId} which belongs to another user", 
                    _currentUser.UserId, request.WorkflowId);
                return null;
            }

            // Get all nodes for the workflow
            var nodes = await _nodeRepository.GetByWorkflowIdAsync(request.WorkflowId, cancellationToken);
            
            // Map to DTOs
            var nodeDtos = nodes.Select(n => new WorkflowNodeDto(
                n.Id,
                n.NodeType,
                n.NodeKind,
                n.Label,
                n.X,
                n.Y,
                n.ConfigJson
            )).ToList();

            _logger.LogInformation("Retrieved {Count} nodes for workflow {WorkflowId}", nodeDtos.Count, request.WorkflowId);
            
            return nodeDtos;
        }
    }
}