using Elsa.Workflows;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modules.Workflow.Workflows;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Workflow.Features.WorkflowExecutions.RunSampleBrowserWorkflow
{
    public class RunSampleBrowserWorkflowHandler : IRequestHandler<RunSampleBrowserWorkflowCommand, RunSampleBrowserWorkflowResult>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<RunSampleBrowserWorkflowHandler> _logger;

        public RunSampleBrowserWorkflowHandler(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<RunSampleBrowserWorkflowHandler> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        public async Task<RunSampleBrowserWorkflowResult> Handle(RunSampleBrowserWorkflowCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting sample browser workflow execution");
                
                using var scope = _serviceScopeFactory.CreateScope();
                var workflowRunner = scope.ServiceProvider.GetRequiredService<IWorkflowRunner>();
                
                // Create a new instance of the workflow directly
                var workflow = ActivatorUtilities.CreateInstance<SampleBrowserWorkflow>(scope.ServiceProvider);
                
                // Run the workflow
                var result = await workflowRunner.RunAsync(workflow);
                
                string instanceId = result.WorkflowState.Id;
                _logger.LogInformation("Successfully executed browser automation workflow with instance ID {InstanceId}", instanceId);

                return new RunSampleBrowserWorkflowResult
                {
                    Success = true,
                    Message = "Browser automation workflow executed successfully",
                    WorkflowInstanceId = instanceId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing browser workflow: {ErrorMessage}", ex.Message);
                
                return new RunSampleBrowserWorkflowResult
                {
                    Success = false,
                    Message = $"Error executing browser workflow: {ex.Message}"
                };
            }
        }
    }
} 