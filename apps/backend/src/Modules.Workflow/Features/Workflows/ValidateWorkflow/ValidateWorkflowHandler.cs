using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Entities;
using Modules.Workflow.DDD.Interfaces;
using Modules.Workflow.Services.Validation;
using ZenFlow.Shared.Application.Auth;
using System.Collections.Generic; // Added for HashSet and Queue
using System.Linq; // Added for LINQ methods

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
            var nodes = (await _nodeRepository.GetByWorkflowIdAsync(request.WorkflowId, cancellationToken)).ToList();
            var edges = (await _edgeRepository.GetByWorkflowIdAsync(request.WorkflowId, cancellationToken)).ToList();

            // Validate nodes and edges
            response = ValidateWorkflowStructure(workflow, nodes, edges); // Renamed for clarity

            return response;
        }

        // Renamed method and updated logic
        private ValidateWorkflowResponse ValidateWorkflowStructure(DDD.Entities.Workflow workflow, List<WorkflowNode> nodes, List<WorkflowEdge> edges)
        {
            var response = new ValidateWorkflowResponse { IsValid = true };
            var nodeMap = nodes.ToDictionary(n => n.Id); // For quick lookup
            var edgeMap = edges.GroupBy(e => e.SourceNodeId).ToDictionary(g => g.Key, g => g.ToList()); // Adjacency list

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
                return response; // Early exit if no nodes
            }

            // Check if workflow has at least one TRIGGER node
            var triggerNodes = nodes.Where(n => n.NodeKind == "TRIGGER").ToList();
            if (triggerNodes.Count == 0)
            {
                response.IsValid = false;
                response.ValidationErrors.Add(new ValidationError
                {
                    ErrorCode = "NO_TRIGGER",
                    ErrorMessage = "Workflow must have at least one trigger node",
                    Severity = "error"
                });
                // Don't return early, continue other checks
            }

            // --- Refined Reachability Check (Replaces old ISOLATED_NODE check) ---
            var reachableNodes = new HashSet<Guid>();
            var queue = new Queue<Guid>(triggerNodes.Select(n => n.Id));

            foreach(var triggerId in triggerNodes.Select(n => n.Id))
            {
                reachableNodes.Add(triggerId);
            }

            while (queue.Count > 0)
            {
                var currentNodeId = queue.Dequeue();

                if (edgeMap.TryGetValue(currentNodeId, out var outgoingEdges))
                {
                    foreach (var edge in outgoingEdges)
                    {
                        if (reachableNodes.Add(edge.TargetNodeId)) // Add returns true if item was added (not already present)
                        {
                            queue.Enqueue(edge.TargetNodeId);
                        }
                    }
                }
            }

            // Check for nodes not reachable from any trigger
            foreach (var node in nodes)
            {
                if (!reachableNodes.Contains(node.Id))
                {
                    response.IsValid = false; // Unreachable nodes make the workflow invalid
                    response.ValidationErrors.Add(new ValidationError
                    {
                        NodeId = node.Id.ToString(),
                        ErrorCode = "UNREACHABLE_NODE", // Changed ErrorCode
                        ErrorMessage = $"Node '{node.Label}' is not reachable from any trigger node",
                        Severity = "error"
                    });
                }
            }
            // --- End of Reachability Check ---


            // Individual Node Checks (Config, Terminal)
            foreach (var node in nodes)
            {
                 // Check if node is a terminal node (Warning only)
                var hasOutgoingEdges = edgeMap.ContainsKey(node.Id) && edgeMap[node.Id].Any();
                // Allow ACTION nodes to be terminal without warning, adjust if other types can be terminal
                if (!hasOutgoingEdges && node.NodeKind != "ACTION" && node.NodeKind != "TRIGGER") // Triggers might not have outgoing edges if workflow is just a trigger
                {
                    // Only a warning for non-terminal nodes without outgoing edges
                    response.ValidationErrors.Add(new ValidationError
                    {
                        NodeId = node.Id.ToString(),
                        ErrorCode = "TERMINAL_NODE",
                        ErrorMessage = $"Node '{node.Label}' has no outgoing connections",
                        Severity = "warning" // Keep as warning
                    });
                }

                // Validate node configuration
                if (!_configValidator.ValidateConfig(node.NodeType, node.ConfigJson, out var configErrors))
                {
                    response.IsValid = false; // Invalid config makes workflow invalid
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

            // Check for cycles (existing logic seems okay, assuming it handles LOOP nodes correctly)
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

            // Final determination of IsValid based on errors found
            response.IsValid = !response.ValidationErrors.Any(e => e.Severity == "error");

            return response;
        }

        // Updated HasCycles method
        private bool HasCycles(List<WorkflowNode> nodes, List<WorkflowEdge> edges)
        {
            var visited = new HashSet<Guid>();
            var recStack = new HashSet<Guid>();
            var nodeMap = nodes.ToDictionary(n => n.Id); // Needed for node kind lookup

            // Start DFS from each node to detect all cycles, not just those reachable from triggers
            foreach (var node in nodes)
            {
                 if (!visited.Contains(node.Id))
                 {
                    if (IsCyclicUtil(node.Id, visited, recStack, edges, nodeMap)) // Pass nodeMap
                        return true;
                 }
            }

            return false;
        }

        // Updated IsCyclicUtil method
        private bool IsCyclicUtil(Guid nodeId, HashSet<Guid> visited, HashSet<Guid> recStack, List<WorkflowEdge> edges, Dictionary<Guid, WorkflowNode> nodeMap)
        {
            if (!nodeMap.ContainsKey(nodeId)) return false; // Node might not exist if graph is inconsistent

            if (recStack.Contains(nodeId)) // If node is already in the current recursion stack, cycle detected
            {
                 // Optional: Check if the cycle involves a LOOP node - if so, might not be an error depending on rules
                 // var node = nodeMap[nodeId];
                 // if (node.NodeKind != "LOOP") return true; // Only return true if it's not a loop node cycle
                 return true; // Simple check: any cycle is reported
            }

            if (visited.Contains(nodeId)) // If node was already visited (and processed) in a previous DFS path, no need to re-check
            {
                return false;
            }

            visited.Add(nodeId);
            recStack.Add(nodeId);

            var node = nodeMap[nodeId];
            // If specific LOOP node handling is needed, add logic here based on node.NodeKind

            var children = edges.Where(e => e.SourceNodeId == nodeId).Select(e => e.TargetNodeId);
            foreach (var child in children)
            {
                if (IsCyclicUtil(child, visited, recStack, edges, nodeMap))
                    return true;
            }


            recStack.Remove(nodeId); // Remove node from recursion stack before returning
            return false;
        }
    }
}