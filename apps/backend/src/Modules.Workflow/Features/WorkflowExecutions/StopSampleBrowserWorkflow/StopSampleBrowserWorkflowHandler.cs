using Elsa.Workflows;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Workflow.Features.WorkflowExecutions.StopSampleBrowserWorkflow
{
    public class StopSampleBrowserWorkflowHandler : IRequestHandler<StopSampleBrowserWorkflowCommand, StopSampleBrowserWorkflowResult>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<StopSampleBrowserWorkflowHandler> _logger;

        public StopSampleBrowserWorkflowHandler(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<StopSampleBrowserWorkflowHandler> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        public async Task<StopSampleBrowserWorkflowResult> Handle(StopSampleBrowserWorkflowCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Stopping browser workflow execution with instance ID {InstanceId}", request.WorkflowInstanceId);
                
                using var scope = _serviceScopeFactory.CreateScope();
                
                // Get necessary services
                var workflowRunner = scope.ServiceProvider.GetRequiredService<IWorkflowRunner>();
                var browserSessionManager = scope.ServiceProvider.GetRequiredService<IBrowserSessionManager>();
                
                // Clean up any browser resources associated with this workflow
                await browserSessionManager.CleanupWorkflowResourcesAsync(request.WorkflowInstanceId);
                
                // Since we don't have direct access to workflow instance management in Elsa 3.0,
                // we'll log a success message but note the limitation
                _logger.LogInformation("Cleaned up browser resources for workflow {WorkflowInstanceId}", 
                    request.WorkflowInstanceId);
                
                return new StopSampleBrowserWorkflowResult
                {
                    Success = true,
                    Message = "Browser automation resources cleaned up successfully. Note: Elsa workflow instance may still be running.",
                    WorkflowInstanceId = request.WorkflowInstanceId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping browser workflow {WorkflowInstanceId}: {ErrorMessage}", 
                    request.WorkflowInstanceId, ex.Message);
                
                return new StopSampleBrowserWorkflowResult
                {
                    Success = false,
                    Message = $"Error stopping browser workflow: {ex.Message}",
                    WorkflowInstanceId = request.WorkflowInstanceId
                };
            }
        }
    }
} 