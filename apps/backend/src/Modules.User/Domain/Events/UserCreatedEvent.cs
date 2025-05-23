﻿using ZenFlow.Shared.Domain;

namespace Modules.User.Domain.Events
{
    public class UserCreatedEvent : DomainEvent
    {
        public Guid UserId { get; }
        public string Username { get; }
        public UserCreatedEvent(Guid userId, string username)
        {
            UserId = userId;
            Username = username;
        }
    }
}
