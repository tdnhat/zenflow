using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Events;
using ZenFlow.Shared.Infrastructure.Logging;

namespace Modules.Workflow.Features.Workflows
{
    public class WorkflowArchivedHandler : DomainEventLoggingHandler<WorkflowArchivedEvent>
    {
        private readonly ILogger<WorkflowArchivedHandler> _logger;

        public WorkflowArchivedHandler(ILogger<WorkflowArchivedHandler> logger)
            : base(logger)
        {
            _logger = logger;
        }

        public override Task Handle(WorkflowArchivedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Workflow with ID {WorkflowId} has been archived.", notification.WorkflowId);
            return Task.CompletedTask;
        }
    }
}