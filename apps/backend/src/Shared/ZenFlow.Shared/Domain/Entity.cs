namespace ZenFlow.Shared.Domain
{
    public abstract class Entity<T> : IEntity<T>, ISoftDelete
    {
        public required T Id { get; set; }

        // Audit properties
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string? LastModifiedBy { get; set; }

        // Soft delete properties
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }
}
