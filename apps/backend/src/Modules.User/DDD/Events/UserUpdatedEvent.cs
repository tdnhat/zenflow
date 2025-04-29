using ZenFlow.Shared.Domain;

namespace Modules.User.DDD.Events
{
    public class UserUpdatedEvent : DomainEvent
    {
        public Guid UserId { get; }
        public string NewUsername { get; }
        public string NewEmail { get; }

        public UserUpdatedEvent(Guid userId, string newUsername, string newEmail)
        {
            UserId = userId;
            NewUsername = newUsername;
            NewEmail = newEmail;
        }
    }
}
