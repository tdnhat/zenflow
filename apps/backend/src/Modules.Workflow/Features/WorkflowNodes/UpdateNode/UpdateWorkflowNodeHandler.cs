using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Interfaces;
using Modules.Workflow.Dtos;
using ZenFlow.Shared.Application.Auth;

namespace Modules.Workflow.Features.WorkflowNodes.UpdateNode
{
    public class UpdateWorkflowNodeHandler : IRequestHandler<UpdateWorkflowNodeCommand, WorkflowNodeDto?>
    {
        private readonly IWorkflowNodeRepository _nodeRepository;
        private readonly IWorkflowRepository _workflowRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<UpdateWorkflowNodeHandler> _logger;

        public UpdateWorkflowNodeHandler(
            IWorkflowNodeRepository nodeRepository,
            IWorkflowRepository workflowRepository,
            ICurrentUserService currentUser,
            ILogger<UpdateWorkflowNodeHandler> logger)
        {
            _nodeRepository = nodeRepository;
            _workflowRepository = workflowRepository;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<WorkflowNodeDto?> Handle(UpdateWorkflowNodeCommand request, CancellationToken cancellationToken)
        {
            // Verify the workflow exists and belongs to the current user
            var workflow = await _workflowRepository.GetByIdAsync(request.WorkflowId, cancellationToken);
            
            if (workflow == null)
            {
                _logger.LogWarning("Workflow with ID {WorkflowId} not found", request.WorkflowId);
                return null;
            }

            // Security check: ensure the user can only modify their own workflows
            if (workflow.CreatedBy != _currentUser.UserId)
            {
                _logger.LogWarning("User {UserId} attempted to update a node in workflow {WorkflowId} which belongs to another user", 
                    _currentUser.UserId, request.WorkflowId);
                return null;
            }

            // Get the node to update
            var node = await _nodeRepository.GetByIdAsync(request.NodeId, cancellationToken);
            
            if (node == null)
            {
                _logger.LogWarning("Node with ID {NodeId} not found in workflow {WorkflowId}", 
                    request.NodeId, request.WorkflowId);
                return null;
            }

            // Verify node belongs to the specified workflow
            if (node.WorkflowId != request.WorkflowId)
            {
                _logger.LogWarning("Node with ID {NodeId} does not belong to workflow {WorkflowId}", 
                    request.NodeId, request.WorkflowId);
                return null;
            }

            // Update the node
            node.Update(request.X, request.Y, request.Label, request.ConfigJson);
            await _nodeRepository.UpdateAsync(node, cancellationToken);

            _logger.LogInformation("Node {NodeId} updated in workflow {WorkflowId} by user {UserId}", 
                node.Id, node.WorkflowId, _currentUser.UserId);

            return new WorkflowNodeDto(
                node.Id,
                node.NodeType,
                node.NodeKind,
                node.Label,
                node.X,
                node.Y,
                node.ConfigJson
            );
        }
    }
}