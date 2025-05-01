using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Interfaces;
using Modules.Workflow.Dtos;
using ZenFlow.Shared.Application.Auth;

namespace Modules.Workflow.Features.Workflows.GetWorkflowById
{
    public class GetWorkflowByIdHandler : IRequestHandler<GetWorkflowByIdQuery, WorkflowDetailDto?>
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<GetWorkflowByIdHandler> _logger;

        public GetWorkflowByIdHandler(
            IWorkflowRepository workflowRepository,
            ICurrentUserService currentUser,
            ILogger<GetWorkflowByIdHandler> logger)
        {
            _workflowRepository = workflowRepository;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<WorkflowDetailDto?> Handle(GetWorkflowByIdQuery request, CancellationToken cancellationToken)
        {
            var workflow = await _workflowRepository.GetByIdWithNodesAndEdgesAsync(request.Id, cancellationToken);

            if (workflow == null)
            {
                _logger.LogWarning("Workflow with ID {WorkflowId} not found", request.Id);
                return null;
            }

            // Security check: ensure the user can only access their own workflows
            if (workflow.CreatedBy != _currentUser.UserId)
            {
                _logger.LogWarning("User {UserId} attempted to access workflow {WorkflowId} which belongs to another user",
                    _currentUser.UserId, request.Id);
                return null;
            }

            _logger.LogInformation("Retrieved workflow {WorkflowId} for user {UserId}", workflow.Id, _currentUser.UserId);

            return new WorkflowDetailDto(
                workflow.Id,
                workflow.Name,
                workflow.Description,
                workflow.Status,
                workflow.Nodes.Select(n => new WorkflowNodeDto(
                    n.Id,
                    n.NodeType,
                    n.NodeKind,
                    n.Label,
                    n.X,
                    n.Y,
                    n.ConfigJson)),
                workflow.Edges.Select(e => new WorkflowEdgeDto(
                    e.Id,
                    e.SourceNodeId,
                    e.TargetNodeId,
                    e.Label,
                    e.EdgeType,
                    e.ConditionJson,
                    e.SourceHandle,
                    e.TargetHandle)));
        }
    }
}