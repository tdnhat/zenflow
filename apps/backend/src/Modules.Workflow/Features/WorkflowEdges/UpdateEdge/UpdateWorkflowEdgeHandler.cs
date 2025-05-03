using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Interfaces;
using Modules.Workflow.Dtos;
using ZenFlow.Shared.Application.Auth;

namespace Modules.Workflow.Features.WorkflowEdges.UpdateEdge
{
    public class UpdateWorkflowEdgeHandler : IRequestHandler<UpdateWorkflowEdgeCommand, WorkflowEdgeDto?>
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<UpdateWorkflowEdgeHandler> _logger;

        public UpdateWorkflowEdgeHandler(
            IWorkflowRepository workflowRepository,
            ICurrentUserService currentUser,
            ILogger<UpdateWorkflowEdgeHandler> logger)
        {
            _workflowRepository = workflowRepository;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<WorkflowEdgeDto?> Handle(UpdateWorkflowEdgeCommand request, CancellationToken cancellationToken)
        {
            // Verify the workflow exists and belongs to the current user
            var workflow = await _workflowRepository.GetByIdWithNodesAndEdgesAsync(request.WorkflowId, cancellationToken);
            
            if (workflow == null)
            {
                _logger.LogWarning("Workflow with ID {WorkflowId} not found", request.WorkflowId);
                return null;
            }

            // Security check: ensure the user can only modify their own workflows
            if (workflow.CreatedBy != _currentUser.UserId)
            {
                _logger.LogWarning("User {UserId} attempted to update an edge in workflow {WorkflowId} which belongs to another user", 
                    _currentUser.UserId, request.WorkflowId);
                return null;
            }

            // Find the edge in the workflow
            var edge = workflow.Edges.FirstOrDefault(e => e.Id == request.EdgeId);
            
            if (edge == null)
            {
                _logger.LogWarning("Edge with ID {EdgeId} not found in workflow {WorkflowId}", 
                    request.EdgeId, request.WorkflowId);
                return null;
            }

            // Update the edge through the aggregate root
            workflow.UpdateEdge(
                request.EdgeId,
                request.Label,
                request.EdgeType,
                request.ConditionJson,
                request.SourceHandle,
                request.TargetHandle
            );
            
            // Save changes to the workflow aggregate
            await _workflowRepository.UpdateAsync(workflow, cancellationToken);
            await _workflowRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Edge {EdgeId} updated in workflow {WorkflowId} by user {UserId}", 
                edge.Id, edge.WorkflowId, _currentUser.UserId);

            return new WorkflowEdgeDto(
                edge.Id,
                edge.SourceNodeId,
                edge.TargetNodeId,
                edge.Label,
                edge.EdgeType,
                edge.ConditionJson,
                edge.SourceHandle,
                edge.TargetHandle
            );
        }
    }
}