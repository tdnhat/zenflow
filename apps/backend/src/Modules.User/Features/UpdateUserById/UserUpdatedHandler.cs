using Microsoft.Extensions.Logging;
using Modules.User.DDD.Events;
using ZenFlow.Shared.Infrastructure.Logging;

namespace Modules.User.Features.UpdateUserById
{
    public class UserUpdatedHandler : DomainEventLoggingHandler<UserUpdatedEvent>
    {
        private readonly ILogger<UserUpdatedHandler> _logger;

        public UserUpdatedHandler(ILogger<UserUpdatedHandler> logger)
            : base(logger)
        {
            _logger = logger;
        }

        public override Task Handle(UserUpdatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("User with ID {UserId} has been updated.", notification.UserId);
            return Task.CompletedTask;
        }
    }
}
