using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Events;
using ZenFlow.Shared.Infrastructure.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Workflow.Features.Workflows
{
    public class WorkflowUpdatedHandler : DomainEventLoggingHandler<WorkflowUpdatedEvent>
    {
        private readonly ILogger<WorkflowUpdatedHandler> _logger;

        public WorkflowUpdatedHandler(ILogger<WorkflowUpdatedHandler> logger)
            : base(logger)
        {
            _logger = logger;
        }

        public override Task Handle(WorkflowUpdatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Workflow with ID {WorkflowId} has been updated.", notification.WorkflowId);
            
            // Don't throw exceptions during event handling to prevent transaction failures
            try
            {
                // Additional handling logic can go here
                
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling WorkflowUpdatedEvent for workflow {WorkflowId}", 
                    notification.WorkflowId);
                return Task.CompletedTask;
            }
        }
    }
}