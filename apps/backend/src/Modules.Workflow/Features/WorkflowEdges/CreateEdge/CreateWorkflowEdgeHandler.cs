using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Entities;
using Modules.Workflow.DDD.Interfaces;
using Modules.Workflow.Dtos;
using ZenFlow.Shared.Application.Auth;

namespace Modules.Workflow.Features.WorkflowEdges.CreateEdge
{
    public class CreateWorkflowEdgeHandler : IRequestHandler<CreateWorkflowEdgeCommand, WorkflowEdgeDto>
    {
        private readonly IWorkflowEdgeRepository _edgeRepository;
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IWorkflowNodeRepository _nodeRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<CreateWorkflowEdgeHandler> _logger;

        public CreateWorkflowEdgeHandler(
            IWorkflowEdgeRepository edgeRepository,
            IWorkflowRepository workflowRepository,
            IWorkflowNodeRepository nodeRepository,
            ICurrentUserService currentUser,
            ILogger<CreateWorkflowEdgeHandler> logger)
        {
            _edgeRepository = edgeRepository;
            _workflowRepository = workflowRepository;
            _nodeRepository = nodeRepository;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<WorkflowEdgeDto> Handle(CreateWorkflowEdgeCommand request, CancellationToken cancellationToken)
        {
            // Verify the workflow exists and belongs to the current user
            var workflow = await _workflowRepository.GetByIdAsync(request.WorkflowId, cancellationToken);
            
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

            // Verify source node exists and belongs to the workflow
            var sourceNode = await _nodeRepository.GetByIdAsync(request.SourceNodeId, cancellationToken);
            if (sourceNode == null || sourceNode.WorkflowId != request.WorkflowId)
            {
                _logger.LogWarning("Source node {NodeId} not found in workflow {WorkflowId}", 
                    request.SourceNodeId, request.WorkflowId);
                throw new InvalidOperationException($"Source node {request.SourceNodeId} not found in workflow {request.WorkflowId}");
            }

            // Verify target node exists and belongs to the workflow
            var targetNode = await _nodeRepository.GetByIdAsync(request.TargetNodeId, cancellationToken);
            if (targetNode == null || targetNode.WorkflowId != request.WorkflowId)
            {
                _logger.LogWarning("Target node {NodeId} not found in workflow {WorkflowId}", 
                    request.TargetNodeId, request.WorkflowId);
                throw new InvalidOperationException($"Target node {request.TargetNodeId} not found in workflow {request.WorkflowId}");
            }

            // Create edge
            var edge = WorkflowEdge.Create(
                request.WorkflowId,
                request.SourceNodeId,
                request.TargetNodeId,
                request.Label,
                request.EdgeType,
                request.ConditionJson,
                request.SourceHandle,
                request.TargetHandle
            );

            await _edgeRepository.AddAsync(edge, cancellationToken);

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