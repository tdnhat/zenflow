using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Events;
using ZenFlow.Shared.Infrastructure.Logging;

namespace Modules.Workflow.Features.WorkflowEdges
{
    public class WorkflowEdgeCreatedHandler : DomainEventLoggingHandler<WorkflowEdgeCreatedEvent>
    {
        private readonly ILogger<WorkflowEdgeCreatedHandler> _logger;

        public WorkflowEdgeCreatedHandler(ILogger<WorkflowEdgeCreatedHandler> logger)
            : base(logger)
        {
            _logger = logger;
        }

        public override Task Handle(WorkflowEdgeCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Edge with ID {EdgeId} has been created between nodes {SourceNodeId} and {TargetNodeId} in workflow {WorkflowId}.", 
                notification.EdgeId, notification.SourceNodeId, notification.TargetNodeId, notification.WorkflowId);
            return Task.CompletedTask;
        }
    }
}