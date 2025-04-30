using ZenFlow.Shared.Domain;

namespace Modules.User.DDD.Events
{
    public class UserDeletedEvent : DomainEvent
    {
        public Guid UserId { get; }
        public string Username { get; }

        public UserDeletedEvent(Guid userId, string username)
        {
            UserId = userId;
            Username = username;
        }
    }
}
