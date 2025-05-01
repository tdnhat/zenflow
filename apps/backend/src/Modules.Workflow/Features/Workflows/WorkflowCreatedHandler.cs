using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Events;
using ZenFlow.Shared.Infrastructure.Logging;

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
            return Task.CompletedTask;
        }
    }
}
