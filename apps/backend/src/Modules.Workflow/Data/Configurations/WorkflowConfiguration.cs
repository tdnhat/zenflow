using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Workflow.DDD.Entities;
using Modules.Workflow.DDD.ValueObjects;

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

            // Configure Status enum to be stored as a string in the database
            builder.Property(w => w.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasConversion(
                    v => v.ToStringValue(),
                    v => WorkflowStatusExtensions.FromString(v));

            // Configure Version as a concurrency token
            builder.Property(w => w.Version)
                .IsRequired()
                .IsConcurrencyToken();

            builder.Ignore(w => w.DomainEvents); // Ignore domain events
        }
    }
}
