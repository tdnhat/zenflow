namespace Modules.User.Domain.Entities
{
    public class UserOutboxMessage
    {
        public Guid Id { get; set; }
        public string EventType { get; set; } = default!;
        public string EventContent { get; set; } = default!;
        public DateTime OccurredOn { get; set; }
        public DateTime? ProcessedOn { get; set; }
        public string? Error { get; set; }
        public int RetryCount { get; set; }
    }
}
