using System.Threading.Tasks;
using ZenFlow.Shared.Domain;

namespace Modules.User.Infrastructure.Events
{
    /// <summary>
    /// Service for publishing user domain events
    /// </summary>
    public interface IUserDomainEventService
    {
        /// <summary>
        /// Publishes a domain event
        /// </summary>
        /// <param name="domainEvent">The domain event to publish</param>
        Task PublishAsync(IDomainEvent domainEvent);
    }
}