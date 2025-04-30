using Microsoft.Extensions.Logging;
using Modules.User.DDD.Events;
using ZenFlow.Shared.Infrastructure.Logging;

namespace Modules.User.Features.CreateUser
{
    public class UserCreatedHandler : DomainEventLoggingHandler<UserCreatedEvent>
    {
        private readonly ILogger<UserCreatedHandler> _logger;

        public UserCreatedHandler(ILogger<UserCreatedHandler> logger)
            : base(logger)
        {
            _logger = logger;
        }

        public override Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("User with ID {UserId} has been created.", notification.UserId);
            return Task.CompletedTask;
        }
    }
}
