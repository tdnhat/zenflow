using Modules.User.DDD.Events;
using ZenFlow.Shared.Domain;

namespace Modules.User.DDD.Entities
{
    public class User : Aggregate<Guid>
    {
        public string ExternalId { get; private set; } = default!;
        public string Username { get; private set; } = default!;
        public string Email { get; private set; } = default!;
        public User() { }
        public static User Create(string externalId, string username, string email)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                ExternalId = externalId,
                Username = username,
                Email = email
            };

            // Raise domain event
            user.AddDomainEvent(new UserCreatedEvent(user.Id, user.Username));
            return user;
        }
    }
}
