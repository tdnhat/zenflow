using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Modules.User.Data.Configurations
{
    public class OutboxMessageConfiguration : IEntityTypeConfiguration<DDD.Entities.UserOutboxMessage>
    {
        public void Configure(EntityTypeBuilder<DDD.Entities.UserOutboxMessage> builder)
        {
            builder.ToTable("OutboxMessages", "user");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.EventType).IsRequired();
            builder.Property(e => e.EventContent).IsRequired();
            builder.Property(e => e.OccurredOn).IsRequired();
            builder.Property(e => e.RetryCount).HasDefaultValue(0);
        }
    }
}
