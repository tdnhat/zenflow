using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Entities;
using Modules.Workflow.DDD.Interfaces;
using Modules.Workflow.Infrastructure.Outbox;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using ZenFlow.Shared.Domain;

namespace Modules.Workflow.Infrastructure.Events
{
    /// <summary>
    /// Implementation of IWorkflowDomainEventService that stores events in the workflow outbox and publishes them in-process
    /// </summary>
    public class WorkflowDomainEventService : IWorkflowDomainEventService
    {
        private readonly IMediator _mediator;
        private readonly IWorkflowOutboxRepository _outboxRepository;
        private readonly ILogger<WorkflowDomainEventService> _logger;

        public WorkflowDomainEventService(
            IMediator mediator,
            IWorkflowOutboxRepository outboxRepository,
            ILogger<WorkflowDomainEventService> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _outboxRepository = outboxRepository ?? throw new ArgumentNullException(nameof(outboxRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task PublishAsync(IDomainEvent domainEvent)
        {
            _logger.LogInformation("Storing workflow domain event {EventType} with ID {EventId} in outbox",
                domainEvent.GetType().Name, domainEvent.EventId);

            // 1. Store in outbox for reliable messaging
            try
            {
                await _outboxRepository.AddAsync(new WorkflowOutboxMessage
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
                _logger.LogError(ex, "Failed to store workflow domain event {EventType} in outbox", domainEvent.GetType().Name);
                throw;
            }

            // 2. Publish in-process for immediate handling
            try
            {
                await _mediator.Publish(domainEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish workflow domain event {EventType} in-process", domainEvent.GetType().Name);
                // We don't rethrow here, as the event is already in the outbox for later processing
            }
        }
    }
}