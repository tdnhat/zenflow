using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Events;
using ZenFlow.Shared.Infrastructure.Logging;

namespace Modules.Workflow.Features.UpdateWorkflow
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
            return Task.CompletedTask;
        }
    }
}