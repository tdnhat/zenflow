using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Events;
using Modules.Workflow.DDD.Events.WorkflowEdgeEvents;
using ZenFlow.Shared.Infrastructure.Logging;

namespace Modules.Workflow.Features.WorkflowEdges
{
    public class WorkflowEdgeDeletedHandler : DomainEventLoggingHandler<WorkflowEdgeDeletedEvent>
    {
        private readonly ILogger<WorkflowEdgeDeletedHandler> _logger;

        public WorkflowEdgeDeletedHandler(ILogger<WorkflowEdgeDeletedHandler> logger)
            : base(logger)
        {
            _logger = logger;
        }

        public override Task Handle(WorkflowEdgeDeletedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Edge with ID {EdgeId} has been deleted from workflow {WorkflowId}.", 
                notification.EdgeId, notification.WorkflowId);
            return Task.CompletedTask;
        }
    }
}