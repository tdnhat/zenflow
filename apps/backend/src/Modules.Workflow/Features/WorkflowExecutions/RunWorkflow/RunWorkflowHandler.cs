using Elsa.Workflows;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Modules.Workflow.DDD.Interfaces;
using Modules.Workflow.DDD.ValueObjects;
using Modules.Workflow.Workflows;
using System;
using System.Collections.Generic;
using System.Linq;
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
                
                // Create a new scope for the background task
                using var scope = _serviceScopeFactory.CreateScope();
                
                // Get fresh repository instances from the new scope
                var workflowRepository = scope.ServiceProvider.GetRequiredService<IWorkflowRepository>();
                var executionRepository = scope.ServiceProvider.GetRequiredService<IWorkflowExecutionRepository>();
                
                // Load the workflow and execution from the database
                var workflow = await workflowRepository.GetByIdWithNodesAndEdgesAsync(workflowId, CancellationToken.None);
                var execution = await executionRepository.GetByIdAsync(executionId, CancellationToken.None);
                
                if (workflow == null || execution == null)
                {
                    _logger.LogError("Workflow or execution not found when trying to execute workflow");
                    return;
                }
                
                // Here you would convert your workflow model to an Elsa workflow and execute it
                // For now, we'll just simulate workflow execution
                await Task.Delay(1000); // Simulate workflow processing time
                
                // Update execution status to completed
                execution.Complete();
                await executionRepository.UpdateAsync(execution, CancellationToken.None);
                
                _logger.LogInformation("Workflow {WorkflowId} execution {ExecutionId} completed successfully", 
                    workflowId, executionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while executing workflow {WorkflowId}, execution {ExecutionId}: {ErrorMessage}", 
                    workflowId, executionId, ex.Message);
                
                try
                {
                    // Create a new scope for error handling
                    using var scope = _serviceScopeFactory.CreateScope();
                    var executionRepository = scope.ServiceProvider.GetRequiredService<IWorkflowExecutionRepository>();
                    
                    // Load the execution record
                    var execution = await executionRepository.GetByIdAsync(executionId, CancellationToken.None);
                    
                    if (execution != null)
                    {
                        // Update execution status to failed
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
    }
} 