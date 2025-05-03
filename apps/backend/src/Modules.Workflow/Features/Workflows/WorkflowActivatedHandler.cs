using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Events;
using Modules.Workflow.DDD.Events.WorkflowEdgeEvents;
using ZenFlow.Shared.Infrastructure.Logging;

namespace Modules.Workflow.Features.Workflows
{
    public class WorkflowActivatedHandler : DomainEventLoggingHandler<WorkflowActivatedEvent>
    {
        private readonly ILogger<WorkflowActivatedHandler> _logger;

        public WorkflowActivatedHandler(ILogger<WorkflowActivatedHandler> logger)
            : base(logger)
        {
            _logger = logger;
        }

        public override Task Handle(WorkflowActivatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Workflow {WorkflowId} activated at {ActivatedAt} with status {Status}", 
                notification.WorkflowId, notification.ActivatedAt, notification.Status);
            return Task.CompletedTask;
        }
    }
}