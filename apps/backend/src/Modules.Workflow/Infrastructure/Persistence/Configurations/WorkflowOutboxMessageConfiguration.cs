using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Workflow.DDD.Entities;

namespace Modules.Workflow.Infrastructure.Persistence.Configurations
{
    public class WorkflowOutboxMessageConfiguration : IEntityTypeConfiguration<WorkflowOutboxMessage>
    {
        public void Configure(EntityTypeBuilder<WorkflowOutboxMessage> builder)
        {
            builder.ToTable("OutboxMessages", "workflow");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.EventType).IsRequired();
            builder.Property(e => e.EventContent).IsRequired();
            builder.Property(e => e.OccurredOn).IsRequired();
            builder.Property(e => e.RetryCount).HasDefaultValue(0);
        }
    }
}
