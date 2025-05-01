using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Interfaces;
using ZenFlow.Shared.Application.Auth;

namespace Modules.Workflow.Features.WorkflowEdges.DeleteEdge
{
    public class DeleteWorkflowEdgeHandler : IRequestHandler<DeleteWorkflowEdgeCommand, bool>
    {
        private readonly IWorkflowEdgeRepository _edgeRepository;
        private readonly IWorkflowRepository _workflowRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<DeleteWorkflowEdgeHandler> _logger;

        public DeleteWorkflowEdgeHandler(
            IWorkflowEdgeRepository edgeRepository,
            IWorkflowRepository workflowRepository,
            ICurrentUserService currentUser,
            ILogger<DeleteWorkflowEdgeHandler> logger)
        {
            _edgeRepository = edgeRepository;
            _workflowRepository = workflowRepository;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<bool> Handle(DeleteWorkflowEdgeCommand request, CancellationToken cancellationToken)
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
                _logger.LogWarning("User {UserId} attempted to delete an edge from workflow {WorkflowId} which belongs to another user", 
                    _currentUser.UserId, request.WorkflowId);
                return false;
            }

            // Get the edge
            var edge = await _edgeRepository.GetByIdAsync(request.EdgeId, cancellationToken);
            
            if (edge == null)
            {
                _logger.LogWarning("Edge with ID {EdgeId} not found", request.EdgeId);
                return false;
            }

            // Verify edge belongs to the specified workflow
            if (edge.WorkflowId != request.WorkflowId)
            {
                _logger.LogWarning("Edge with ID {EdgeId} does not belong to workflow {WorkflowId}", 
                    request.EdgeId, request.WorkflowId);
                return false;
            }

            // Delete the edge
            await _edgeRepository.DeleteAsync(edge, cancellationToken);

            _logger.LogInformation("Edge {EdgeId} deleted from workflow {WorkflowId} by user {UserId}", 
                edge.Id, edge.WorkflowId, _currentUser.UserId);
            
            return true;
        }
    }
}