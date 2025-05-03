using Elsa.Workflows;
using Elsa.Workflows.Activities;
using Elsa.Workflows.Builders;
using Elsa.Workflows.Models;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Modules.Workflow.DDD.Entities;
using Modules.Workflow.DDD.Interfaces;
using Modules.Workflow.DDD.ValueObjects;
using Modules.Workflow.Services.BrowserAutomation.Activities;
using Modules.Workflow.Workflows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Workflow.Features.WorkflowExecutions.RunWorkflow
{
    public class RunWorkflowHandler : IRequestHandler<RunWorkflowCommand, RunWorkflowResult>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<RunWorkflowHandler> _logger;
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IWorkflowExecutionRepository _executionRepository;

        public RunWorkflowHandler(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<RunWorkflowHandler> logger,
            IWorkflowRepository workflowRepository,
            IWorkflowExecutionRepository executionRepository)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _workflowRepository = workflowRepository;
            _executionRepository = executionRepository;
        }

        public async Task<RunWorkflowResult> Handle(RunWorkflowCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting workflow execution for workflow ID {WorkflowId}", request.WorkflowId);
                
                // Check if the workflow ID is for the sample browser workflow
                if (request.WorkflowId.Equals("sample-browser", StringComparison.OrdinalIgnoreCase))
                {
                    return await RunSampleBrowserWorkflow(request);
                }
                
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
        
        private async Task<RunWorkflowResult> RunSampleBrowserWorkflow(RunWorkflowCommand request)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var workflowRunner = scope.ServiceProvider.GetRequiredService<IWorkflowRunner>();
            
            // Apply custom options if provided
            var optionsBuilder = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<SampleBrowserWorkflowOptions>>();
            var options = optionsBuilder.CurrentValue;

            // Override options with request parameters if provided
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                options.SearchTerm = request.SearchTerm;
                _logger.LogInformation("Using custom search term: {SearchTerm}", options.SearchTerm);
            }

            if (request.TakeScreenshots.HasValue)
            {
                options.EnableScreenshots = request.TakeScreenshots.Value;
                _logger.LogInformation("Screenshots enabled: {EnableScreenshots}", options.EnableScreenshots);
            }

            // Create workflow with custom options
            var workflowWithOptions = ActivatorUtilities.CreateInstance<SampleBrowserWorkflow>(
                scope.ServiceProvider,
                Options.Create(options));

            // Run the workflow
            var result = await workflowRunner.RunAsync(workflowWithOptions);
            string instanceId = result.WorkflowState.Id;
            
            _logger.LogInformation("Successfully executed browser automation workflow with instance ID {InstanceId}", instanceId);

            return new RunWorkflowResult
            {
                Success = true,
                Message = "Browser automation workflow executed successfully",
                ExecutionId = instanceId,
                Status = "Running"
            };
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
                    _logger.LogInformation("Workflow execution completed");
                    execution.Complete();
                    await executionRepository.UpdateAsync(execution, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while executing workflow {WorkflowId}: {ErrorMessage}", 
                        workflowId, ex.Message);
                    execution.Fail(ex.Message, ex.StackTrace);
                    await executionRepository.UpdateAsync(execution, CancellationToken.None);
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
            
            switch (normalizedType)
            {
                case "navigate":
                    var nav = ActivatorUtilities.CreateInstance<NavigateActivity>(serviceProvider);
                    nav.Url = config.TryGetValue("url", out var url) ? url?.ToString() ?? string.Empty : string.Empty;
                    nav.Timeout = config.TryGetValue("timeout", out var timeout) && int.TryParse(timeout?.ToString(), out var t) ? t : 60000;
                    nav.WaitUntil = config.TryGetValue("waitUntil", out var waitUntil) ? waitUntil?.ToString() ?? "load" : "load";
                    return nav;
                case "click":
                    var click = ActivatorUtilities.CreateInstance<ClickActivity>(serviceProvider);
                    click.Selector = config.TryGetValue("selector", out var selector) ? selector?.ToString() ?? string.Empty : string.Empty;
                    click.RequireVisible = config.TryGetValue("requireVisible", out var requireVisible) && bool.TryParse(requireVisible?.ToString(), out var rv) ? rv : true;
                    click.Delay = config.TryGetValue("delay", out var delay) && int.TryParse(delay?.ToString(), out var d) ? d : 0;
                    click.Force = config.TryGetValue("force", out var force) && bool.TryParse(force?.ToString(), out var f) ? f : false;
                    click.AfterDelay = config.TryGetValue("afterDelay", out var afterDelay) && int.TryParse(afterDelay?.ToString(), out var ad) ? ad : 0;
                    return click;
                case "input":
                    var inputText = ActivatorUtilities.CreateInstance<InputTextActivity>(serviceProvider);
                    inputText.Selector = config.TryGetValue("selector", out var sel) ? sel?.ToString() ?? string.Empty : string.Empty;
                    inputText.Text = config.TryGetValue("text", out var text) ? text?.ToString() ?? string.Empty : string.Empty;
                    inputText.TypeDelay = config.TryGetValue("typeDelay", out var td) && int.TryParse(td?.ToString(), out var tdi) ? tdi : 0;
                    inputText.ClearFirst = config.TryGetValue("clearFirst", out var cf) && bool.TryParse(cf?.ToString(), out var cfb) ? cfb : true;
                    return inputText;
                case "screenshot":
                    var screenshot = ActivatorUtilities.CreateInstance<ScreenshotActivity>(serviceProvider);
                    screenshot.FullPage = config.TryGetValue("fullPage", out var fp) && bool.TryParse(fp?.ToString(), out var fpb) ? fpb : false;
                    screenshot.Selector = config.TryGetValue("selector", out var ss) ? ss?.ToString() : null;
                    return screenshot;
                case "extractdata":
                    var extract = ActivatorUtilities.CreateInstance<ExtractDataActivity>(serviceProvider);
                    extract.Selector = config.TryGetValue("selector", out var es) ? es?.ToString() ?? string.Empty : string.Empty;
                    extract.PropertyToExtract = config.TryGetValue("propertyToExtract", out var pte) ? pte?.ToString() ?? "innerText" : "innerText";
                    extract.ExtractAll = config.TryGetValue("extractAll", out var ea) && bool.TryParse(ea?.ToString(), out var eab) ? eab : false;
                    extract.OutputVariableName = config.TryGetValue("outputVariableName", out var ovn) ? ovn?.ToString() ?? "extractedData" : "extractedData";
                    return extract;
                case "waitforselector":
                    var wait = ActivatorUtilities.CreateInstance<WaitForSelectorActivity>(serviceProvider);
                    wait.Selector = config.TryGetValue("selector", out var ws) ? ws?.ToString() ?? string.Empty : string.Empty;
                    wait.Timeout = config.TryGetValue("timeout", out var wt) && int.TryParse(wt?.ToString(), out var wti) ? wti : 30000;
                    return wait;
                default:
                    throw new NotSupportedException($"Node type '{node.NodeType}' is not supported.");
            }
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