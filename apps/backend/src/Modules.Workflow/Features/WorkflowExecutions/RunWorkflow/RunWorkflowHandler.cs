using Elsa.Workflows;
using Elsa.Workflows.Activities;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Entities;
using Modules.Workflow.DDD.Interfaces;
using Modules.Workflow.Features.WorkflowExecutions.RunWorkflow.ActivityMappers;
using Modules.Workflow.Services.BrowserAutomation.Activities;
using System.Text.Json;

namespace Modules.Workflow.Features.WorkflowExecutions.RunWorkflow
{
    public class RunWorkflowHandler : IRequestHandler<RunWorkflowCommand, RunWorkflowResult>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<RunWorkflowHandler> _logger;
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IWorkflowExecutionRepository _executionRepository;
        private readonly IActivityMapperFactory _activityMapperFactory;

        public RunWorkflowHandler(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<RunWorkflowHandler> logger,
            IWorkflowRepository workflowRepository,
            IWorkflowExecutionRepository executionRepository,
            IActivityMapperFactory activityMapperFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _workflowRepository = workflowRepository;
            _executionRepository = executionRepository;
            _activityMapperFactory = activityMapperFactory;
        }

        public async Task<RunWorkflowResult> Handle(RunWorkflowCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting workflow execution for workflow ID {WorkflowId}", request.WorkflowId);

                // Load the workflow from the database
                var workflowId = Guid.Parse(request.WorkflowId);
                var workflow = await _workflowRepository.GetByIdWithNodesAndEdgesAsync(workflowId, cancellationToken);

                if (workflow == null)
                {
                    _logger.LogWarning("Workflow with ID {WorkflowId} not found", request.WorkflowId);
                    return new RunWorkflowResult
                    {
                        Success = false,
                        Message = $"Workflow with ID {request.WorkflowId} not found"
                    };
                }

                // Create a workflow execution record
                var workflowExecution = DDD.Entities.WorkflowExecution.Create(workflow.Id);
                workflowExecution.Start();

                await _executionRepository.AddAsync(workflowExecution, cancellationToken);
                await _executionRepository.SaveChangesAsync(cancellationToken);

                // Cache the execution ID and workflow ID to use in the background task
                var executionId = workflowExecution.Id;
                var capturedWorkflowId = workflow.Id;

                // Start executing the workflow in a background task
                _ = ExecuteWorkflowAsync(capturedWorkflowId, executionId, request.Input);

                return new RunWorkflowResult
                {
                    Success = true,
                    Message = "Workflow execution started",
                    ExecutionId = workflowExecution.Id.ToString(),
                    Status = workflowExecution.Status
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing workflow {WorkflowId}: {ErrorMessage}",
                    request.WorkflowId, ex.Message);

                return new RunWorkflowResult
                {
                    Success = false,
                    Message = $"Error executing workflow: {ex.Message}"
                };
            }
        }


        private async Task ExecuteWorkflowAsync(Guid workflowId, Guid executionId, Dictionary<string, object>? input)
        {
            try
            {
                _logger.LogInformation("Executing workflow {WorkflowId}, execution {ExecutionId}",
                    workflowId, executionId);
                using var scope = _serviceScopeFactory.CreateScope();
                var workflowRepository = scope.ServiceProvider.GetRequiredService<IWorkflowRepository>();
                var executionRepository = scope.ServiceProvider.GetRequiredService<IWorkflowExecutionRepository>();
                var workflow = await workflowRepository.GetByIdWithNodesAndEdgesAsync(workflowId, CancellationToken.None);
                var execution = await executionRepository.GetByIdAsync(executionId, CancellationToken.None);
                if (workflow == null || execution == null)
                {
                    _logger.LogError("Workflow or execution not found when trying to execute workflow");
                    return;
                }
                try
                {
                    var workflowRunner = scope.ServiceProvider.GetRequiredService<IWorkflowRunner>();
                    var serviceProvider = scope.ServiceProvider;
                    var orderedNodes = GetOrderedNodes(workflow);
                    var activities = new List<IActivity>();
                    foreach (var node in orderedNodes)
                    {
                        try
                        {
                            activities.Add(MapNodeToActivity(node, serviceProvider));
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to map node {NodeId} to activity: {Error}", node.Id, ex.Message);
                            throw;
                        }
                    }
                    var activityVisitor = serviceProvider.GetRequiredService<IActivityVisitor>();
                    var identityGraphService = serviceProvider.GetRequiredService<IIdentityGraphService>();
                    var activityRegistry = serviceProvider.GetRequiredService<IActivityRegistry>();
                    var identityGenerator = serviceProvider.GetRequiredService<IIdentityGenerator>();

                    // Create a workflow definition directly with a Sequence as the root
                    var workflowDefinition = new Sequence { Activities = activities };

                    // Log if we have input variables
                    if (input != null && input.Count > 0)
                    {
                        _logger.LogInformation("Workflow has {Count} input variables", input.Count);
                    }

                    // Run the workflow
                    var workflowInstance = await workflowRunner.RunAsync(workflowDefinition);

                    if (workflowInstance?.WorkflowState?.Id != null)
                    {
                        execution.SetExternalWorkflowId(workflowInstance.WorkflowState.Id);
                    }

                    // Store output if workflow completed successfully
                    if (workflowInstance?.WorkflowState?.Output != null)
                    {
                        var output = workflowInstance.WorkflowState.Output;
                        var serializedOutput = JsonSerializer.Serialize(output);
                        execution.StoreOutput(serializedOutput);
                        _logger.LogInformation("Workflow execution output: {Output}", serializedOutput);
                    }

                    _logger.LogInformation("Workflow execution completed");
                    execution.Complete();
                    await executionRepository.UpdateAsync(execution, CancellationToken.None);
                    await executionRepository.SaveChangesAsync(CancellationToken.None);
                    _logger.LogInformation("Workflow execution status updated to completed");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while executing workflow {WorkflowId}: {ErrorMessage}",
                        workflowId, ex.Message);
                    execution.Fail(ex.Message, ex.StackTrace);
                    await executionRepository.UpdateAsync(execution, CancellationToken.None);
                    await executionRepository.SaveChangesAsync(CancellationToken.None);
                    _logger.LogInformation("Workflow execution status updated to failed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while executing workflow {WorkflowId}, execution {ExecutionId}: {ErrorMessage}",
                    workflowId, executionId, ex.Message);
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var executionRepository = scope.ServiceProvider.GetRequiredService<IWorkflowExecutionRepository>();
                    var execution = await executionRepository.GetByIdAsync(executionId, CancellationToken.None);
                    if (execution != null)
                    {
                        execution.Fail(ex.Message, ex.StackTrace);
                        await executionRepository.UpdateAsync(execution, CancellationToken.None);
                        await executionRepository.SaveChangesAsync(CancellationToken.None);
                        _logger.LogInformation("Workflow execution status updated to failed after error: {ErrorMessage}", ex.Message);
                    }
                }
                catch (Exception updateEx)
                {
                    _logger.LogError(updateEx, "Failed to update execution status after error: {ErrorMessage}", updateEx.Message);
                }
            }
        }

        private IActivity MapNodeToActivity(WorkflowNode node, IServiceProvider serviceProvider)
        {
            var config = JsonSerializer.Deserialize<Dictionary<string, object>>(node.ConfigJson ?? "{}") ?? new();

            // Normalize the node type by removing "Activity" suffix and converting to lowercase
            var normalizedType = node.NodeType
                .Replace("Activity", "", StringComparison.OrdinalIgnoreCase)
                .ToLowerInvariant();

            // Get the appropriate mapper for this activity type
            var mapper = _activityMapperFactory.GetMapper(normalizedType);
            if (mapper == null)
            {
                throw new NotSupportedException($"Node type '{node.NodeType}' is not supported.");
            }

            // Use the mapper to create and configure the activity
            return mapper.MapToActivity(normalizedType, config, serviceProvider);
        }

        private List<WorkflowNode> GetOrderedNodes(DDD.Entities.Workflow workflow)
        {
            // Simple topological sort: start with nodes that have no incoming edges
            var nodes = workflow.Nodes.ToDictionary(n => n.Id, n => n);
            var edges = workflow.Edges;
            var incoming = new Dictionary<Guid, int>();
            foreach (var node in nodes.Values)
                incoming[node.Id] = 0;
            foreach (var edge in edges)
                if (incoming.ContainsKey(edge.TargetNodeId))
                    incoming[edge.TargetNodeId]++;
            var queue = new Queue<Guid>(incoming.Where(kv => kv.Value == 0).Select(kv => kv.Key));
            var ordered = new List<WorkflowNode>();
            var visited = new HashSet<Guid>();
            while (queue.Count > 0)
            {
                var id = queue.Dequeue();
                if (!visited.Add(id)) continue;
                if (nodes.TryGetValue(id, out var node))
                    ordered.Add(node);
                foreach (var edge in edges.Where(e => e.SourceNodeId == id))
                {
                    if (incoming.ContainsKey(edge.TargetNodeId))
                    {
                        incoming[edge.TargetNodeId]--;
                        if (incoming[edge.TargetNodeId] == 0)
                            queue.Enqueue(edge.TargetNodeId);
                    }
                }
            }
            return ordered;
        }
    }
}