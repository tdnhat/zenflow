using MediatR;
using Microsoft.Extensions.Logging;
using ZenFlow.Shared.Domain;

namespace ZenFlow.Shared.Infrastructure.Logging
{
    public class DomainEventLoggingHandler<TEvent> : INotificationHandler<TEvent>
        where TEvent : IDomainEvent
    {
        private readonly ILogger<DomainEventLoggingHandler<TEvent>> _logger;
        public DomainEventLoggingHandler(ILogger<DomainEventLoggingHandler<TEvent>> logger)
        {
            _logger = logger;
        }
        public virtual Task Handle(TEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[DomainEvent] {Event}", typeof(TEvent).Name);
            return Task.CompletedTask;
        }
    }
}
