using Microsoft.Extensions.Logging;
using Modules.Workflow.Domain.Core;
using Modules.Workflow.Domain.Enums;
using Modules.Workflow.Domain.Interfaces.Core;
using Modules.Workflow.Domain.Entities;

namespace Modules.Workflow.Infrastructure.Services.Workflow
{
    public class WorkflowEngine : IWorkflowEngine
    {
        private readonly ILogger<WorkflowEngine> _logger;
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IWorkflowInstanceRepository _instanceRepository;
        private readonly IEnumerable<IActivityExecutor> _activityExecutors;

        public WorkflowEngine(
            ILogger<WorkflowEngine> logger,
            IWorkflowRepository workflowRepository,
            IWorkflowInstanceRepository instanceRepository,
            IEnumerable<IActivityExecutor> activityExecutors)
        {
            _logger = logger;
            _workflowRepository = workflowRepository;
            _instanceRepository = instanceRepository;
            _activityExecutors = activityExecutors;
        }

        public async Task<Guid> StartWorkflowAsync(
            Guid workflowDefinitionId,
            Dictionary<string, object>? initialVariables = null,
            CancellationToken cancellationToken = default)
        {
            // Load the workflow definition
            var workflowDefinition = await _workflowRepository.GetByIdAsync(workflowDefinitionId, cancellationToken);
            if (workflowDefinition == null)
                throw new ArgumentException($"Workflow definition not found: {workflowDefinitionId}");

            // Create a new workflow instance
            var workflowInstanceId = Guid.NewGuid();
            var executionContext = new WorkflowExecutionContext
            {
                WorkflowInstanceId = workflowInstanceId,
                WorkflowDefinitionId = workflowDefinitionId,
                Variables = initialVariables ?? new Dictionary<string, object>(),
                Status = WorkflowStatus.NotStarted
            };

            // Initialize node execution contexts
            foreach (var node in workflowDefinition.Nodes)
            {
                executionContext.NodeExecutions[node.Id.ToString()] = new NodeExecutionContext
                {
                    NodeId = node.Id.ToString(),
                    ActivityType = node.ActivityType,
                    Status = WorkflowNodeStatus.NotStarted
                };
            }

            // Save the initial state
            await _instanceRepository.SaveAsync(executionContext, cancellationToken);

            // Start the workflow execution
            await ExecuteWorkflowAsync(executionContext, Guid.Empty, cancellationToken);

            return workflowInstanceId;
        }

        public async Task ResumeWorkflowAsync(
            Guid workflowInstanceId,
            Guid nodeId,
            Dictionary<string, object>? outputData = null,
            CancellationToken cancellationToken = default)
        {
            // Load the workflow instance
            var executionContext = await _instanceRepository.GetByIdAsync(workflowInstanceId, cancellationToken);
            if (executionContext == null)
                throw new ArgumentException($"Workflow instance not found: {workflowInstanceId}");

            // Check if the workflow is suspended
            if (executionContext.Status != WorkflowStatus.Suspended)
                throw new InvalidOperationException($"Cannot resume workflow in state: {executionContext.Status}");

            // Check if the node exists
            if (!executionContext.NodeExecutions.TryGetValue(nodeId.ToString(), out var nodeContext))
                throw new ArgumentException($"Node not found in workflow: {nodeId}");

            // Check if the node is in the correct state
            if (nodeContext.Status != WorkflowNodeStatus.Running)
                throw new InvalidOperationException($"Cannot resume node in state: {nodeContext.Status}");

            // Update the node with the output data
            if (outputData != null)
            {
                foreach (var (key, value) in outputData)
                {
                    nodeContext.OutputData[key] = value;
                }
            }

            // Mark the node as completed
            nodeContext.Status = WorkflowNodeStatus.Completed;
            nodeContext.CompletedAt = DateTime.UtcNow;
            nodeContext.AddLog("Node resumed and completed");

            // Save the updated state
            await _instanceRepository.SaveAsync(executionContext, cancellationToken);

            // Continue workflow execution
            await ExecuteWorkflowAsync(executionContext, nodeId, cancellationToken);
        }

        public async Task CancelWorkflowAsync(
            Guid workflowInstanceId,
            CancellationToken cancellationToken = default)
        {
            // Load the workflow instance
            var executionContext = await _instanceRepository.GetByIdAsync(workflowInstanceId, cancellationToken);
            if (executionContext == null)
                throw new ArgumentException($"Workflow instance not found: {workflowInstanceId}");

            // Check if the workflow can be cancelled
            if (executionContext.Status is WorkflowStatus.Completed or WorkflowStatus.Failed or WorkflowStatus.Cancelled)
                throw new InvalidOperationException($"Cannot cancel workflow in state: {executionContext.Status}");

            // Mark the workflow as cancelled
            executionContext.Status = WorkflowStatus.Cancelled;
            executionContext.CompletedAt = DateTime.UtcNow;

            // Mark all running nodes as cancelled
            foreach (var (_, nodeContext) in executionContext.NodeExecutions)
            {
                if (nodeContext.Status == WorkflowNodeStatus.Running)
                {
                    nodeContext.Status = WorkflowNodeStatus.Cancelled;
                    nodeContext.CompletedAt = DateTime.UtcNow;
                    nodeContext.AddLog("Node cancelled due to workflow cancellation");
                }
            }

            // Save the updated state
            await _instanceRepository.SaveAsync(executionContext, cancellationToken);
        }

        public async Task<WorkflowExecutionContext> GetWorkflowStateAsync(
            Guid workflowInstanceId,
            CancellationToken cancellationToken = default)
        {
            return await _instanceRepository.GetByIdAsync(workflowInstanceId, cancellationToken);
        }

        private async Task ExecuteWorkflowAsync(
            WorkflowExecutionContext context,
            Guid currentNodeId,
            CancellationToken cancellationToken)
        {
            // Load the workflow definition
            var workflowDefinition = await _workflowRepository.GetByIdAsync(context.WorkflowDefinitionId, cancellationToken);
            if (workflowDefinition == null)
                throw new InvalidOperationException($"Workflow definition not found: {context.WorkflowDefinitionId}");

            // Update workflow status
            if (context.Status == WorkflowStatus.NotStarted)
            {
                context.Status = WorkflowStatus.Running;
                context.StartedAt = DateTime.UtcNow;
            }
            else if (context.Status == WorkflowStatus.Suspended)
            {
                context.Status = WorkflowStatus.Running;
            }

            // Save the updated state
            await _instanceRepository.SaveAsync(context, cancellationToken);

            // Determine the next nodes to execute
            var nodesToExecute = new List<Guid>();

            if (currentNodeId == Guid.Empty)
            {
                // Starting the workflow - find nodes with no incoming edges
                nodesToExecute.AddRange(FindStartNodes(workflowDefinition));
            }
            else
            {
                // Continuing from a completed node - find outgoing edges
                nodesToExecute.AddRange(FindNextNodes(workflowDefinition, currentNodeId));
            }

            // Execute each node
            bool workflowSuspended = false;

            foreach (var nodeId in nodesToExecute)
            {
                var result = await ExecuteNodeAsync(context, workflowDefinition, nodeId, cancellationToken);

                if (result == ActivityExecutionResult.Suspended)
                {
                    workflowSuspended = true;
                    break; // Stop execution when a node suspends
                }
                else if (result == ActivityExecutionResult.Failed)
                {
                    // Node failed - mark workflow as failed
                    context.Status = WorkflowStatus.Failed;
                    context.CompletedAt = DateTime.UtcNow;
                    context.Error = $"Node {nodeId} failed";
                    await _instanceRepository.SaveAsync(context, cancellationToken);
                    return;
                }

                // Continue with outgoing nodes from this one
                var nextNodes = FindNextNodes(workflowDefinition, nodeId);
                foreach (var nextNodeId in nextNodes)
                {
                    // Check if all incoming edges to this node are from completed nodes
                    if (AreAllIncomingNodesCompleted(context, workflowDefinition, nextNodeId))
                    {
                        var nextResult = await ExecuteNodeAsync(context, workflowDefinition, nextNodeId, cancellationToken);

                        if (nextResult == ActivityExecutionResult.Suspended)
                        {
                            workflowSuspended = true;
                            break; // Stop execution when a node suspends
                        }
                        else if (nextResult == ActivityExecutionResult.Failed)
                        {
                            // Node failed - mark workflow as failed
                            context.Status = WorkflowStatus.Failed;
                            context.CompletedAt = DateTime.UtcNow;
                            context.Error = $"Node {nextNodeId} failed";
                            await _instanceRepository.SaveAsync(context, cancellationToken);
                            return;
                        }
                    }
                }

                if (workflowSuspended)
                    break;
            }

            // Check if the workflow is complete (all nodes are completed or skipped)
            if (!workflowSuspended && !context.NodeExecutions.Values.Any(n =>
                n.Status is WorkflowNodeStatus.NotStarted or WorkflowNodeStatus.Running))
            {
                context.Status = WorkflowStatus.Completed;
                context.CompletedAt = DateTime.UtcNow;
            }
            else if (workflowSuspended)
            {
                context.Status = WorkflowStatus.Suspended;
            }

            // Save the final state
            await _instanceRepository.SaveAsync(context, cancellationToken);
        }

        private async Task<ActivityExecutionResult> ExecuteNodeAsync(
            WorkflowExecutionContext context,
            WorkflowDefinition workflowDefinition,
            Guid nodeId,
            CancellationToken cancellationToken)
        {
            // Get the node from the workflow definition
            var node = workflowDefinition.Nodes.FirstOrDefault(n => n.Id == nodeId);
            if (node == null)
                throw new ArgumentException($"Node not found in workflow definition: {nodeId}");

            // Get the node execution context
            if (!context.NodeExecutions.TryGetValue(nodeId.ToString(), out var nodeContext))
                throw new InvalidOperationException($"Node execution context not found: {nodeId}");

            // Skip if already completed or failed
            if (nodeContext.Status is WorkflowNodeStatus.Completed or WorkflowNodeStatus.Failed or WorkflowNodeStatus.Skipped)
                return ActivityExecutionResult.Completed;

            // Update node status
            nodeContext.Status = WorkflowNodeStatus.Running;
            nodeContext.StartedAt = DateTime.UtcNow;
            nodeContext.AddLog("Node execution started");

            // Save the updated state
            await _instanceRepository.SaveAsync(context, cancellationToken);

            // Find the appropriate activity executor
            var activityExecutor = _activityExecutors.FirstOrDefault(e => e.CanExecute(node.ActivityType));
            if (activityExecutor == null)
                throw new InvalidOperationException($"No executor found for activity type: {node.ActivityType}");

            // Prepare input data from incoming edges
            var inputData = PrepareInputData(context, workflowDefinition, nodeId);

            // Execute the activity
            var (result, outputData) = await activityExecutor.ExecuteAsync(
                node.ActivityType,
                node.ActivityProperties,
                inputData,
                context,
                nodeContext,
                cancellationToken);

            // Update node status based on result
            if (result == ActivityExecutionResult.Completed)
            {
                nodeContext.Status = WorkflowNodeStatus.Completed;
                nodeContext.CompletedAt = DateTime.UtcNow;
                nodeContext.AddLog("Node execution completed successfully");

                // Store output data
                foreach (var (key, value) in outputData)
                {
                    nodeContext.OutputData[key] = value;
                }
            }
            else if (result == ActivityExecutionResult.Failed)
            {
                nodeContext.Status = WorkflowNodeStatus.Failed;
                nodeContext.CompletedAt = DateTime.UtcNow;
                nodeContext.AddLog("Node execution failed");

                // Store error information
                if (outputData.TryGetValue("Error", out var error))
                    nodeContext.Error = error?.ToString();
            }
            // For Suspended, we leave the status as Running

            // Save the updated state
            await _instanceRepository.SaveAsync(context, cancellationToken);

            return result;
        }

        private List<Guid> FindStartNodes(WorkflowDefinition workflowDefinition)
        {
            // Find nodes with no incoming edges
            var nodesWithIncomingEdges = workflowDefinition.Edges
                .Select(e => e.Target)
                .Distinct()
                .ToHashSet();

            return workflowDefinition.Nodes
                .Where(n => !nodesWithIncomingEdges.Contains(n.Id))
                .Select(n => n.Id)
                .ToList();
        }

        private List<Guid> FindNextNodes(WorkflowDefinition workflowDefinition, Guid nodeId)
        {
            // Find nodes connected by outgoing edges
            return workflowDefinition.Edges
                .Where(e => e.Source == nodeId)
                .Select(e => e.Target)
                .ToList();
        }

        private bool AreAllIncomingNodesCompleted(
            WorkflowExecutionContext context,
            WorkflowDefinition workflowDefinition,
            Guid nodeId)
        {
            // Find all nodes with edges pointing to this node
            var incomingNodeIds = workflowDefinition.Edges
                .Where(e => e.Target == nodeId)
                .Select(e => e.Source)
                .ToList();

            // Check if all incoming nodes are completed
            return incomingNodeIds.All(id =>
                context.NodeExecutions.TryGetValue(id.ToString(), out var nodeContext) &&
                nodeContext.Status == WorkflowNodeStatus.Completed);
        }

        private Dictionary<string, object> PrepareInputData(
            WorkflowExecutionContext context,
            WorkflowDefinition workflowDefinition,
            Guid nodeId)
        {
            var inputData = new Dictionary<string, object>();

            // Find all incoming edges
            var incomingEdges = workflowDefinition.Edges
                .Where(e => e.Target == nodeId)
                .ToList();

            // Get the node definition
            var node = workflowDefinition.Nodes.First(n => n.Id == nodeId);

            // Add global variables
            foreach (var (key, value) in context.Variables)
            {
                inputData[key] = value;
            }

            // Add outputs from source nodes based on input mappings
            foreach (var edge in incomingEdges)
            {
                if (context.NodeExecutions.TryGetValue(edge.Source.ToString(), out var sourceNodeContext) &&
                    sourceNodeContext.Status == WorkflowNodeStatus.Completed)
                {
                    // Check if there are input mappings for this edge
                    var inputMappings = node.InputMappings
                        .Where(m => m.SourceNodeId == edge.Source)
                        .ToList();

                    if (inputMappings.Any())
                    {
                        // Apply specific mappings
                        foreach (var mapping in inputMappings)
                        {
                            if (sourceNodeContext.OutputData.TryGetValue(mapping.SourceProperty, out var value))
                            {
                                inputData[mapping.TargetProperty] = value;
                            }
                        }
                    }
                    else
                    {
                        // No specific mappings, copy all outputs
                        foreach (var (key, value) in sourceNodeContext.OutputData)
                        {
                            inputData[key] = value;
                        }
                    }
                }
            }

            return inputData;
        }
    }
}
