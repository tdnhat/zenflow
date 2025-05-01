using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Entities;
using Modules.Workflow.DDD.Interfaces;
using Modules.Workflow.Services.NodeManagement;
using ZenFlow.Shared.Application.Auth;

namespace Modules.Workflow.Features.Workflows.ValidateWorkflow
{
    public class ValidateWorkflowHandler : IRequestHandler<ValidateWorkflowCommand, ValidateWorkflowResponse>
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IWorkflowNodeRepository _nodeRepository;
        private readonly IWorkflowEdgeRepository _edgeRepository;
        private readonly INodeConfigValidator _configValidator;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<ValidateWorkflowHandler> _logger;

        public ValidateWorkflowHandler(
            IWorkflowRepository workflowRepository,
            IWorkflowNodeRepository nodeRepository,
            IWorkflowEdgeRepository edgeRepository,
            INodeConfigValidator configValidator,
            ICurrentUserService currentUser,
            ILogger<ValidateWorkflowHandler> logger)
        {
            _workflowRepository = workflowRepository;
            _nodeRepository = nodeRepository;
            _edgeRepository = edgeRepository;
            _configValidator = configValidator;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<ValidateWorkflowResponse> Handle(ValidateWorkflowCommand request, CancellationToken cancellationToken)
        {
            var response = new ValidateWorkflowResponse { IsValid = true };

            // Get workflow
            var workflow = await _workflowRepository.GetByIdAsync(request.WorkflowId, cancellationToken);
            if (workflow == null)
            {
                response.IsValid = false;
                response.ValidationErrors.Add(new ValidationError
                {
                    ErrorCode = "WORKFLOW_NOT_FOUND",
                    ErrorMessage = $"Workflow with ID {request.WorkflowId} not found",
                    Severity = "error"
                });
                return response;
            }

            // Check permissions
            if (workflow.CreatedBy != _currentUser.UserId)
            {
                response.IsValid = false;
                response.ValidationErrors.Add(new ValidationError
                {
                    ErrorCode = "PERMISSION_DENIED",
                    ErrorMessage = "You don't have permission to validate this workflow",
                    Severity = "error"
                });
                return response;
            }

            // Get nodes and edges
            var nodes = await _nodeRepository.GetByWorkflowIdAsync(request.WorkflowId, cancellationToken);
            var edges = await _edgeRepository.GetByWorkflowIdAsync(request.WorkflowId, cancellationToken);

            // Validate nodes and edges
            response = ValidateWorkflow(workflow, nodes.ToList(), edges.ToList());

            return response;
        }

        private ValidateWorkflowResponse ValidateWorkflow(DDD.Entities.Workflow workflow, List<WorkflowNode> nodes, List<WorkflowEdge> edges)
        {
            var response = new ValidateWorkflowResponse { IsValid = true };

            // Check if workflow has at least one node
            if (nodes.Count == 0)
            {
                response.IsValid = false;
                response.ValidationErrors.Add(new ValidationError
                {
                    ErrorCode = "NO_NODES",
                    ErrorMessage = "Workflow must have at least one node",
                    Severity = "error"
                });
                return response;
            }

            // Check if workflow has at least one TRIGGER node
            var hasTrigger = nodes.Any(n => n.NodeKind == "TRIGGER");
            if (!hasTrigger)
            {
                response.IsValid = false;
                response.ValidationErrors.Add(new ValidationError
                {
                    ErrorCode = "NO_TRIGGER",
                    ErrorMessage = "Workflow must have at least one trigger node",
                    Severity = "error"
                });
            }

            // Check for isolated nodes (no incoming or outgoing edges except for TRIGGER nodes)
            foreach (var node in nodes)
            {
                if (node.NodeKind == "TRIGGER")
                    continue; // Triggers don't need incoming edges

                var incomingEdges = edges.Where(e => e.TargetNodeId == node.Id).ToList();
                var outgoingEdges = edges.Where(e => e.SourceNodeId == node.Id).ToList();

                if (incomingEdges.Count == 0)
                {
                    response.IsValid = false;
                    response.ValidationErrors.Add(new ValidationError
                    {
                        NodeId = node.Id.ToString(),
                        ErrorCode = "ISOLATED_NODE",
                        ErrorMessage = $"Node '{node.Label}' has no incoming connections",
                        Severity = "error"
                    });
                }

                // Check if node is a terminal node
                if (outgoingEdges.Count == 0 && node.NodeKind != "ACTION")
                {
                    // Only a warning for non-terminal nodes without outgoing edges
                    response.ValidationErrors.Add(new ValidationError
                    {
                        NodeId = node.Id.ToString(),
                        ErrorCode = "TERMINAL_NODE",
                        ErrorMessage = $"Node '{node.Label}' has no outgoing connections",
                        Severity = "warning"
                    });
                }

                // Validate node configuration
                if (!_configValidator.ValidateConfig(node.NodeType, node.ConfigJson, out var configErrors))
                {
                    response.IsValid = false;
                    foreach (var error in configErrors)
                    {
                        response.ValidationErrors.Add(new ValidationError
                        {
                            NodeId = node.Id.ToString(),
                            ErrorCode = "INVALID_CONFIG",
                            ErrorMessage = error,
                            Severity = "error"
                        });
                    }
                }
            }

            // Check for cycles (except for LOOP nodes which can have cycles)
            if (HasCycles(nodes, edges))
            {
                response.IsValid = false;
                response.ValidationErrors.Add(new ValidationError
                {
                    ErrorCode = "CYCLE_DETECTED",
                    ErrorMessage = "Workflow contains cycles outside of loop nodes",
                    Severity = "error"
                });
            }

            return response;
        }

        private bool HasCycles(List<WorkflowNode> nodes, List<WorkflowEdge> edges)
        {
            // Simple cycle detection algorithm
            // A more sophisticated solution would ignore cycles involving LOOP nodes
            var visited = new HashSet<Guid>();
            var recStack = new HashSet<Guid>();

            foreach (var node in nodes.Where(n => n.NodeKind == "TRIGGER"))
            {
                if (IsCyclicUtil(node.Id, visited, recStack, edges, nodes))
                    return true;
            }

            return false;
        }

        private bool IsCyclicUtil(Guid nodeId, HashSet<Guid> visited, HashSet<Guid> recStack, List<WorkflowEdge> edges, List<WorkflowNode> nodes)
        {
            if (!visited.Contains(nodeId))
            {
                visited.Add(nodeId);
                recStack.Add(nodeId);

                var node = nodes.FirstOrDefault(n => n.Id == nodeId);
                if (node != null && node.NodeKind == "LOOP")
                {
                    // Skip cycle detection for LOOP nodes as they are allowed to have cycles
                    recStack.Remove(nodeId);
                    return false;
                }

                var children = edges.Where(e => e.SourceNodeId == nodeId).Select(e => e.TargetNodeId);
                foreach (var child in children)
                {
                    if (!visited.Contains(child) && IsCyclicUtil(child, visited, recStack, edges, nodes))
                        return true;
                    else if (recStack.Contains(child))
                        return true;
                }
            }

            recStack.Remove(nodeId);
            return false;
        }
    }
}