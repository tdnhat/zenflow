using MediatR;
using Microsoft.Extensions.Logging;
using Modules.User.Domain.Entities;
using Modules.User.Domain.Interfaces;
using System.Text.Json;
using ZenFlow.Shared.Domain;

namespace Modules.User.Infrastructure.EventHandling.DomainEvents
{
    /// <summary>
    /// Implementation of IUserDomainEventService that stores events in the user outbox and publishes them in-process
    /// </summary>
    public class UserDomainEventService : IUserDomainEventService
    {
        private readonly IMediator _mediator;
        private readonly IUserOutboxRepository _outboxRepository;
        private readonly ILogger<UserDomainEventService> _logger;

        public UserDomainEventService(
            IMediator mediator,
            IUserOutboxRepository outboxRepository,
            ILogger<UserDomainEventService> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _outboxRepository = outboxRepository ?? throw new ArgumentNullException(nameof(outboxRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task PublishAsync(IDomainEvent domainEvent)
        {
            _logger.LogInformation("Storing user domain event {EventType} with ID {EventId} in outbox",
                domainEvent.GetType().Name, domainEvent.EventId);

            // 1. Store in outbox for reliable messaging
            try
            {
                await _outboxRepository.AddAsync(new UserOutboxMessage
                {
                    Id = Guid.NewGuid(),
                    EventType = domainEvent.GetType().AssemblyQualifiedName,
                    EventContent = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                    OccurredOn = DateTime.UtcNow,
                    ProcessedOn = null,
                    RetryCount = 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to store user domain event {EventType} in outbox", domainEvent.GetType().Name);
                throw;
            }

            // 2. Publish in-process for immediate handling
            try
            {
                await _mediator.Publish(domainEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish user domain event {EventType} in-process", domainEvent.GetType().Name);
                // We don't rethrow here, as the event is already in the outbox for later processing
            }
        }
    }
}