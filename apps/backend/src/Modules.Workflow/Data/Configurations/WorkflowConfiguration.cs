using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Modules.Workflow.Data.Configurations
{
    public class WorkflowConfiguration : IEntityTypeConfiguration<DDD.Entities.Workflow>
    {
        public void Configure(EntityTypeBuilder<DDD.Entities.Workflow> builder)
        {
            builder.ToTable("Workflows");

            builder.HasKey(w => w.Id);

            builder.Property(w => w.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(w => w.Description)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(w => w.Status)
                .IsRequired();

            builder.Ignore(w => w.DomainEvents); // Ignore domain events
        }
    }
}
