using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Entities;
using Modules.Workflow.DDD.Interfaces;
using Modules.Workflow.Dtos;
using ZenFlow.Shared.Application.Auth;

namespace Modules.Workflow.Features.WorkflowNodes.CreateNode
{
    public class CreateWorkflowNodeHandler : IRequestHandler<CreateWorkflowNodeCommand, WorkflowNodeDto>
    {
        private readonly IWorkflowNodeRepository _nodeRepository;
        private readonly IWorkflowRepository _workflowRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<CreateWorkflowNodeHandler> _logger;

        public CreateWorkflowNodeHandler(
            IWorkflowNodeRepository nodeRepository,
            IWorkflowRepository workflowRepository,
            ICurrentUserService currentUser,
            ILogger<CreateWorkflowNodeHandler> logger)
        {
            _nodeRepository = nodeRepository;
            _workflowRepository = workflowRepository;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<WorkflowNodeDto> Handle(CreateWorkflowNodeCommand request, CancellationToken cancellationToken)
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
                _logger.LogWarning("User {UserId} attempted to add a node to workflow {WorkflowId} which belongs to another user", 
                    _currentUser.UserId, request.WorkflowId);
                throw new UnauthorizedAccessException("You do not have permission to modify this workflow");
            }

            // Create the workflow node
            var node = WorkflowNode.Create(
                request.WorkflowId,
                request.NodeType,
                request.X,
                request.Y,
                request.Label,
                request.ConfigJson
            );

            await _nodeRepository.AddAsync(node, cancellationToken);

            _logger.LogInformation("Node {NodeId} created in workflow {WorkflowId} by user {UserId}", 
                node.Id, node.WorkflowId, _currentUser.UserId);

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