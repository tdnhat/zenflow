using ZenFlow.Shared.Domain;

namespace Modules.User.Domain.Events
{
    public class UserUpdatedEvent : DomainEvent
    {
        public Guid UserId { get; }
        public string NewUsername { get; }

        public UserUpdatedEvent(Guid userId, string newUsername)
        {
            UserId = userId;
            NewUsername = newUsername;
        }
    }
}
