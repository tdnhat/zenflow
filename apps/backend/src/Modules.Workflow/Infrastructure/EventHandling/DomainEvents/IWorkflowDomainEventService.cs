using ZenFlow.Shared.Domain;

namespace Modules.Workflow.Infrastructure.EventHandling.DomainEvents
{
    /// <summary>
    /// Service for publishing workflow domain events
    /// </summary>
    public interface IWorkflowDomainEventService
    {
        /// <summary>
        /// Publishes a domain event
        /// </summary>
        /// <param name="domainEvent">The domain event to publish</param>
        Task PublishAsync(IDomainEvent domainEvent);
    }
}