using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Interfaces;
using Modules.Workflow.Dtos;
using ZenFlow.Shared.Application.Auth;

namespace Modules.Workflow.Features.WorkflowEdges.CreateEdge
{
    public class CreateWorkflowEdgeHandler : IRequestHandler<CreateWorkflowEdgeCommand, WorkflowEdgeDto>
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<CreateWorkflowEdgeHandler> _logger;

        public CreateWorkflowEdgeHandler(
            IWorkflowRepository workflowRepository,
            ICurrentUserService currentUser,
            ILogger<CreateWorkflowEdgeHandler> logger)
        {
            _workflowRepository = workflowRepository;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<WorkflowEdgeDto> Handle(CreateWorkflowEdgeCommand request, CancellationToken cancellationToken)
        {
            // Verify the workflow exists and belongs to the current user
            var workflow = await _workflowRepository.GetByIdWithNodesAndEdgesAsync(request.WorkflowId, cancellationToken);
            
            if (workflow == null)
            {
                _logger.LogWarning("Workflow with ID {WorkflowId} not found", request.WorkflowId);
                throw new InvalidOperationException($"Workflow with ID {request.WorkflowId} not found");
            }

            // Security check: ensure the user can only modify their own workflows
            if (workflow.CreatedBy != _currentUser.UserId)
            {
                _logger.LogWarning("User {UserId} attempted to add an edge to workflow {WorkflowId} which belongs to another user", 
                    _currentUser.UserId, request.WorkflowId);
                throw new UnauthorizedAccessException("You do not have permission to modify this workflow");
            }

            // Add the edge through the aggregate root, which will perform all necessary validations 
            // and raise the appropriate domain event
            var edge = workflow.AddEdge(
                request.SourceNodeId,
                request.TargetNodeId,
                request.Label,
                request.EdgeType,
                request.ConditionJson,
                request.SourceHandle,
                request.TargetHandle
            );
            
            // Save changes to the workflow aggregate
            await _workflowRepository.UpdateAsync(workflow, cancellationToken);
            await _workflowRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Edge {EdgeId} created between nodes {SourceNodeId} and {TargetNodeId} in workflow {WorkflowId} by user {UserId}", 
                edge.Id, edge.SourceNodeId, edge.TargetNodeId, edge.WorkflowId, _currentUser.UserId);

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