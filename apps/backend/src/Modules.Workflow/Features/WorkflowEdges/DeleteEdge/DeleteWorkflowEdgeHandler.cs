using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Interfaces;
using ZenFlow.Shared.Application.Auth;

namespace Modules.Workflow.Features.WorkflowEdges.DeleteEdge
{
    public class DeleteWorkflowEdgeHandler : IRequestHandler<DeleteWorkflowEdgeCommand, bool>
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<DeleteWorkflowEdgeHandler> _logger;

        public DeleteWorkflowEdgeHandler(
            IWorkflowRepository workflowRepository,
            ICurrentUserService currentUser,
            ILogger<DeleteWorkflowEdgeHandler> logger)
        {
            _workflowRepository = workflowRepository;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<bool> Handle(DeleteWorkflowEdgeCommand request, CancellationToken cancellationToken)
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
                _logger.LogWarning("User {UserId} attempted to delete an edge from workflow {WorkflowId} which belongs to another user", 
                    _currentUser.UserId, request.WorkflowId);
                return false;
            }

            // Find the edge in the workflow
            var edge = workflow.Edges.FirstOrDefault(e => e.Id == request.EdgeId);
            
            if (edge == null)
            {
                _logger.LogWarning("Edge with ID {EdgeId} not found in workflow {WorkflowId}", 
                    request.EdgeId, request.WorkflowId);
                return false;
            }

            // Remove the edge through the aggregate root, which will raise the appropriate domain event
            workflow.RemoveEdge(request.EdgeId);
            
            // Save changes to the workflow aggregate
            await _workflowRepository.UpdateAsync(workflow, cancellationToken);
            await _workflowRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Edge {EdgeId} deleted from workflow {WorkflowId} by user {UserId}", 
                request.EdgeId, request.WorkflowId, _currentUser.UserId);

            return true;
        }
    }
}