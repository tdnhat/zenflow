using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Events;
using Modules.Workflow.DDD.Events.WorkflowEdgeEvents;
using ZenFlow.Shared.Infrastructure.Logging;

namespace Modules.Workflow.Features.WorkflowEdges
{
    public class WorkflowEdgeUpdatedHandler : DomainEventLoggingHandler<WorkflowEdgeUpdatedEvent>
    {
        private readonly ILogger<WorkflowEdgeUpdatedHandler> _logger;

        public WorkflowEdgeUpdatedHandler(ILogger<WorkflowEdgeUpdatedHandler> logger)
            : base(logger)
        {
            _logger = logger;
        }

        public override Task Handle(WorkflowEdgeUpdatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Edge with ID {EdgeId} has been updated in workflow {WorkflowId}.", 
                notification.EdgeId, notification.WorkflowId);
            return Task.CompletedTask;
        }
    }
}