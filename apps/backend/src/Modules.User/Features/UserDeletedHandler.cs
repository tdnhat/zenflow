using MediatR;
using Microsoft.Extensions.Logging;
using Modules.User.Domain.Events;
using ZenFlow.Shared.Infrastructure.Logging;

namespace Modules.User.Features
{
    public class UserDeletedHandler : DomainEventLoggingHandler<UserDeletedEvent>
    {
        private readonly ILogger<UserDeletedHandler> _logger;

        public UserDeletedHandler(ILogger<UserDeletedHandler> logger)
            : base(logger)
        {
            _logger = logger;
        }

        public override Task Handle(UserDeletedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("User with ID {UserId} has been deleted.", notification.UserId);
            return Task.CompletedTask;
        }
    }
}
