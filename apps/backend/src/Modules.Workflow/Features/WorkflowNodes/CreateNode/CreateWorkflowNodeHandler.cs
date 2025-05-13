using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Interfaces;
using ZenFlow.Shared.Application.Auth;

namespace Modules.Workflow.Features.WorkflowNodes.CreateNode
{
    public class CreateWorkflowNodeHandler : IRequestHandler<CreateWorkflowNodeCommand, CreateWorkflowNodeResponse>
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<CreateWorkflowNodeHandler> _logger;

        public CreateWorkflowNodeHandler(
            IWorkflowRepository workflowRepository,
            ICurrentUserService currentUser,
            ILogger<CreateWorkflowNodeHandler> logger)
        {
            _workflowRepository = workflowRepository;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<CreateWorkflowNodeResponse> Handle(CreateWorkflowNodeCommand request, CancellationToken cancellationToken)
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

            // Add the node through the aggregate root, which will raise the appropriate domain event
            var node = workflow.AddNode(
                request.NodeType,
                request.NodeKind,
                request.X,
                request.Y,
                request.Label,
                request.ConfigJson
            );
            
            // Save changes to the workflow aggregate
            await _workflowRepository.UpdateAsync(workflow, cancellationToken);
            await _workflowRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Node {NodeId} created in workflow {WorkflowId} by user {UserId}", 
                node.Id, node.WorkflowId, _currentUser.UserId);

            return new CreateWorkflowNodeResponse(
                node.Id,
                node.NodeType,
                node.NodeKind,
                node.Label,
                node.X,
                node.Y,
                node.ConfigJson);
        }
    }
}