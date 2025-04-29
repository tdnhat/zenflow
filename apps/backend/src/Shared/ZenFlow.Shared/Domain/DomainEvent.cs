namespace ZenFlow.Shared.Domain
{
    public class DomainEvent : IDomainEvent
    {
        public Guid EventId => Guid.NewGuid();
        public DateTime OccurredOn => DateTime.UtcNow;
        public string EventType => GetType().AssemblyQualifiedName!;
    }
}
