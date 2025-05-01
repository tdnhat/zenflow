using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Interfaces;
using Modules.Workflow.Dtos;
using ZenFlow.Shared.Application.Auth;

namespace Modules.Workflow.Features.WorkflowNodes.GetNodeById
{
    public class GetWorkflowNodeByIdHandler : IRequestHandler<GetWorkflowNodeByIdQuery, WorkflowNodeDto?>
    {
        private readonly IWorkflowNodeRepository _nodeRepository;
        private readonly IWorkflowRepository _workflowRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<GetWorkflowNodeByIdHandler> _logger;

        public GetWorkflowNodeByIdHandler(
            IWorkflowNodeRepository nodeRepository,
            IWorkflowRepository workflowRepository,
            ICurrentUserService currentUser,
            ILogger<GetWorkflowNodeByIdHandler> logger)
        {
            _nodeRepository = nodeRepository;
            _workflowRepository = workflowRepository;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<WorkflowNodeDto?> Handle(GetWorkflowNodeByIdQuery request, CancellationToken cancellationToken)
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
                _logger.LogWarning("User {UserId} attempted to access node in workflow {WorkflowId} which belongs to another user", 
                    _currentUser.UserId, request.WorkflowId);
                return null;
            }

            // Get the node
            var node = await _nodeRepository.GetByIdAsync(request.NodeId, cancellationToken);
            
            if (node == null)
            {
                _logger.LogWarning("Node with ID {NodeId} not found", request.NodeId);
                return null;
            }

            // Verify node belongs to the specified workflow
            if (node.WorkflowId != request.WorkflowId)
            {
                _logger.LogWarning("Node with ID {NodeId} does not belong to workflow {WorkflowId}", 
                    request.NodeId, request.WorkflowId);
                return null;
            }

            _logger.LogInformation("Retrieved node {NodeId} for workflow {WorkflowId}", node.Id, node.WorkflowId);
            
            return new WorkflowNodeDto(
                node.Id,
                node.NodeType,
                node.Label,
                node.X,
                node.Y,
                node.ConfigJson
            );
        }
    }
}