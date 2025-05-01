using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Events;
using ZenFlow.Shared.Infrastructure.Logging;

namespace Modules.Workflow.Features.WorkflowNodes
{
    public class WorkflowNodeUpdatedHandler : DomainEventLoggingHandler<WorkflowNodeUpdatedEvent>
    {
        private readonly ILogger<WorkflowNodeUpdatedHandler> _logger;

        public WorkflowNodeUpdatedHandler(ILogger<WorkflowNodeUpdatedHandler> logger)
            : base(logger)
        {
            _logger = logger;
        }

        public override Task Handle(WorkflowNodeUpdatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Node with ID {NodeId} of type {NodeType} has been updated in workflow {WorkflowId}.", 
                notification.NodeId, notification.NodeType, notification.WorkflowId);
            return Task.CompletedTask;
        }
    }
}