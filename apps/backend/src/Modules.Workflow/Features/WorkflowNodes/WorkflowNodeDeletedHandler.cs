using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Events;
using ZenFlow.Shared.Infrastructure.Logging;

namespace Modules.Workflow.Features.WorkflowNodes
{
    public class WorkflowNodeDeletedHandler : DomainEventLoggingHandler<WorkflowNodeDeletedEvent>
    {
        private readonly ILogger<WorkflowNodeDeletedHandler> _logger;

        public WorkflowNodeDeletedHandler(ILogger<WorkflowNodeDeletedHandler> logger)
            : base(logger)
        {
            _logger = logger;
        }

        public override Task Handle(WorkflowNodeDeletedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Node with ID {NodeId} has been deleted from workflow {WorkflowId}.", 
                notification.NodeId, notification.WorkflowId);
            return Task.CompletedTask;
        }
    }
}