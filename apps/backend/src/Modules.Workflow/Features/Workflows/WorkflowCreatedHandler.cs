using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Events;
using ZenFlow.Shared.Infrastructure.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Workflow.Features.Workflows
{
    public class WorkflowCreatedHandler : DomainEventLoggingHandler<WorkflowCreatedEvent>
    {
        private readonly ILogger<WorkflowCreatedHandler> _logger;

        public WorkflowCreatedHandler(ILogger<WorkflowCreatedHandler> logger)
            : base(logger)
        {
            _logger = logger;
        }

        public override Task Handle(WorkflowCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Workflow with ID {WorkflowId} has been created.", notification.WorkflowId);
            
            // Don't throw exceptions during event handling to prevent transaction failures
            try
            {
                // Additional handling logic can go here
                
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling WorkflowCreatedEvent for workflow {WorkflowId}", 
                    notification.WorkflowId);
                return Task.CompletedTask;
            }
        }
    }
}
