using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Interfaces;
using ZenFlow.Shared.Application.Auth;

namespace Modules.Workflow.Features.WorkflowNodes.DeleteNode
{
    public class DeleteWorkflowNodeHandler : IRequestHandler<DeleteWorkflowNodeCommand, bool>
    {
        private readonly IWorkflowNodeRepository _nodeRepository;
        private readonly IWorkflowRepository _workflowRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<DeleteWorkflowNodeHandler> _logger;

        public DeleteWorkflowNodeHandler(
            IWorkflowNodeRepository nodeRepository,
            IWorkflowRepository workflowRepository,
            ICurrentUserService currentUser,
            ILogger<DeleteWorkflowNodeHandler> logger)
        {
            _nodeRepository = nodeRepository;
            _workflowRepository = workflowRepository;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<bool> Handle(DeleteWorkflowNodeCommand request, CancellationToken cancellationToken)
        {
            // Verify the workflow exists and belongs to the current user
            var workflow = await _workflowRepository.GetByIdAsync(request.WorkflowId, cancellationToken);
            
            if (workflow == null)
            {
                _logger.LogWarning("Workflow with ID {WorkflowId} not found", request.WorkflowId);
                return false;
            }

            // Security check: ensure the user can only modify their own workflows
            if (workflow.CreatedBy != _currentUser.UserId)
            {
                _logger.LogWarning("User {UserId} attempted to delete a node from workflow {WorkflowId} which belongs to another user", 
                    _currentUser.UserId, request.WorkflowId);
                return false;
            }

            // Get the node to delete
            var node = await _nodeRepository.GetByIdAsync(request.NodeId, cancellationToken);
            
            if (node == null)
            {
                _logger.LogWarning("Node with ID {NodeId} not found in workflow {WorkflowId}", 
                    request.NodeId, request.WorkflowId);
                return false;
            }

            // Verify node belongs to the specified workflow
            if (node.WorkflowId != request.WorkflowId)
            {
                _logger.LogWarning("Node with ID {NodeId} does not belong to workflow {WorkflowId}", 
                    request.NodeId, request.WorkflowId);
                return false;
            }

            // Mark the node as deleted to trigger domain events
            node.MarkAsDeleted();
            
            // Delete the node
            await _nodeRepository.DeleteAsync(node, cancellationToken);

            _logger.LogInformation("Node {NodeId} deleted from workflow {WorkflowId} by user {UserId}", 
                node.Id, node.WorkflowId, _currentUser.UserId);

            return true;
        }
    }
}