using MediatR;
using Microsoft.Extensions.Logging;
using Modules.User.DDD.Events;

namespace Modules.User.Features.DeleteUserById
{
    public class UserDeletedHandler : INotificationHandler<UserDeletedEvent>
    {
        private readonly ILogger<UserDeletedHandler> _logger;

        public UserDeletedHandler(ILogger<UserDeletedHandler> logger)
        {
            _logger = logger;
        }
        public Task Handle(UserDeletedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("User deleted: {UserId} ({Username})", notification.UserId, notification.Username);
            return Task.CompletedTask;
        }
    }
}
