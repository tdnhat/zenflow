using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Interfaces;
using ZenFlow.Shared.Application.Auth;

namespace Modules.Workflow.Features.WorkflowNodes.DeleteNode
{
    public class DeleteWorkflowNodeHandler : IRequestHandler<DeleteWorkflowNodeCommand, bool>
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<DeleteWorkflowNodeHandler> _logger;

        public DeleteWorkflowNodeHandler(
            IWorkflowRepository workflowRepository,
            ICurrentUserService currentUser,
            ILogger<DeleteWorkflowNodeHandler> logger)
        {
            _workflowRepository = workflowRepository;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<bool> Handle(DeleteWorkflowNodeCommand request, CancellationToken cancellationToken)
        {
            // Verify the workflow exists and belongs to the current user
            var workflow = await _workflowRepository.GetByIdWithNodesAndEdgesAsync(request.WorkflowId, cancellationToken);
            
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

            // Find the node in the workflow
            var node = workflow.Nodes.FirstOrDefault(n => n.Id == request.NodeId);
            
            if (node == null)
            {
                _logger.LogWarning("Node with ID {NodeId} not found in workflow {WorkflowId}", 
                    request.NodeId, request.WorkflowId);
                return false;
            }

            // Remove the node through the aggregate root, which will raise the appropriate domain event
            workflow.RemoveNode(request.NodeId);
            
            // Save changes to the workflow aggregate
            await _workflowRepository.UpdateAsync(workflow, cancellationToken);
            await _workflowRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Node {NodeId} deleted from workflow {WorkflowId} by user {UserId}", 
                request.NodeId, request.WorkflowId, _currentUser.UserId);

            return true;
        }
    }
}