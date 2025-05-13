using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.User.Domain.Entities;

namespace Modules.User.Infrastructure.Persistence.Configurations
{
    public class OutboxMessageConfiguration : IEntityTypeConfiguration<UserOutboxMessage>
    {
        public void Configure(EntityTypeBuilder<UserOutboxMessage> builder)
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
