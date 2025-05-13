using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Interfaces;
using Modules.Workflow.Features.Workflows.Common;
using ZenFlow.Shared.Application.Auth;

namespace Modules.Workflow.Features.WorkflowNodes.UpdateNode
{
    public class UpdateWorkflowNodeHandler : IRequestHandler<UpdateWorkflowNodeCommand, WorkflowNodeResponse?>
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<UpdateWorkflowNodeHandler> _logger;

        public UpdateWorkflowNodeHandler(
            IWorkflowRepository workflowRepository,
            ICurrentUserService currentUser,
            ILogger<UpdateWorkflowNodeHandler> logger)
        {
            _workflowRepository = workflowRepository;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<WorkflowNodeResponse?> Handle(UpdateWorkflowNodeCommand request, CancellationToken cancellationToken)
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
                _logger.LogWarning("User {UserId} attempted to update a node in workflow {WorkflowId} which belongs to another user", 
                    _currentUser.UserId, request.WorkflowId);
                return null;
            }

            // Find the node in the workflow
            var node = workflow.Nodes.FirstOrDefault(n => n.Id == request.NodeId);
            
            if (node == null)
            {
                _logger.LogWarning("Node with ID {NodeId} not found in workflow {WorkflowId}", 
                    request.NodeId, request.WorkflowId);
                return null;
            }

            // Update the node through the aggregate root
            workflow.UpdateNode(request.NodeId, request.X, request.Y, request.Label, request.ConfigJson);
            
            // Save changes to the workflow aggregate
            await _workflowRepository.UpdateAsync(workflow, cancellationToken);
            await _workflowRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Node {NodeId} updated in workflow {WorkflowId} by user {UserId}", 
                node.Id, node.WorkflowId, _currentUser.UserId);

            return new WorkflowNodeResponse(
                node.Id,
                node.NodeType,
                node.NodeKind,
                node.Label,
                node.X,
                node.Y,
                node.ConfigJson
            );
        }
    }
}