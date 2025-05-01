using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Events;
using ZenFlow.Shared.Infrastructure.Logging;

namespace Modules.Workflow.Features.RestoreWorkflow
{
    public class WorkflowRestoredHandler : DomainEventLoggingHandler<WorkflowRestoredEvent>
    {
        private readonly ILogger<WorkflowRestoredHandler> _logger;

        public WorkflowRestoredHandler(ILogger<WorkflowRestoredHandler> logger)
            : base(logger)
        {
            _logger = logger;
        }

        public override Task Handle(WorkflowRestoredEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Workflow with ID {WorkflowId} has been restored.", notification.WorkflowId);
            return Task.CompletedTask;
        }
    }
}