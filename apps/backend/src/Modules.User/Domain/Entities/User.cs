using Modules.User.Domain.Events;
using ZenFlow.Shared.Domain;

namespace Modules.User.Domain.Entities
{
    public class User : Aggregate<Guid>
    {
        public string ExternalId { get; private set; } = default!;
        // Add setter for Username to allow updates
        public string Username { get; set; } = default!;
        public string Email { get; private set; } = default!;
        public User() { } // Parameterless constructor for EF Core
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
        public void Update(string username)
        {
            Username = username;

            // Raise domain event for username change
            AddDomainEvent(new UserUpdatedEvent(Id, username));
        }

        public void MarkDeleted()
        {
            // Raise domain event for user deletion
            AddDomainEvent(new UserDeletedEvent(Id, Username));
        }
    }
}
