using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Events;
using ZenFlow.Shared.Infrastructure.Logging;

namespace Modules.Workflow.Features.WorkflowNodes
{
    public class WorkflowNodeCreatedHandler : DomainEventLoggingHandler<WorkflowNodeCreatedEvent>
    {
        private readonly ILogger<WorkflowNodeCreatedHandler> _logger;

        public WorkflowNodeCreatedHandler(ILogger<WorkflowNodeCreatedHandler> logger)
            : base(logger)
        {
            _logger = logger;
        }

        public override Task Handle(WorkflowNodeCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Node with ID {NodeId} of type {NodeType} has been created in workflow {WorkflowId}.", 
                notification.NodeId, notification.NodeType, notification.WorkflowId);
            return Task.CompletedTask;
        }
    }
}