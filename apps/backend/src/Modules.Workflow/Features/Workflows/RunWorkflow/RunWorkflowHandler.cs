using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Workflow.Domain.Interfaces.Core;

namespace Modules.Workflow.Features.Workflows.RunWorkflow
{
    public class RunWorkflowHandler : IRequestHandler<RunWorkflowCommand, Guid>
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IWorkflowEngine _workflowEngine;
        private readonly ILogger<RunWorkflowHandler> _logger;

        public RunWorkflowHandler(
            IWorkflowRepository workflowRepository,
            IWorkflowEngine workflowEngine,
            ILogger<RunWorkflowHandler> logger)
        {
            _workflowRepository = workflowRepository;
            _workflowEngine = workflowEngine;
            _logger = logger;
        }

        public async Task<Guid> Handle(RunWorkflowCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting workflow execution for workflow ID {WorkflowId}", request.WorkflowId);
            
            // Check if workflow exists
            var workflow = await _workflowRepository.GetByIdAsync(request.WorkflowId, cancellationToken);
            if (workflow == null)
            {
                _logger.LogWarning("Workflow definition with ID {WorkflowId} not found", request.WorkflowId);
                throw new KeyNotFoundException($"Workflow definition with ID {request.WorkflowId} not found");
            }
            
            // Start the workflow execution
            var workflowInstanceId = await _workflowEngine.StartWorkflowAsync(
                request.WorkflowId,
                request.Variables,
                cancellationToken);
                
            _logger.LogInformation("Workflow {WorkflowId} started with instance ID {WorkflowInstanceId}", 
                request.WorkflowId, workflowInstanceId);
                
            return workflowInstanceId;
        }
    }
}