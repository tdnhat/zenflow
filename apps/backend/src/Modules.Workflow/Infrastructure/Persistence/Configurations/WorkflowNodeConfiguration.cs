using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Workflow.Domain.Entities;

namespace Modules.Workflow.Infrastructure.Persistence.Configurations
{
    public class WorkflowNodeConfiguration : IEntityTypeConfiguration<WorkflowNode>
    {
        public void Configure(EntityTypeBuilder<WorkflowNode> builder)
        {
            builder.ToTable("WorkflowNodes");

            builder.HasKey(n => new { n.WorkflowId, n.Id });

            builder.Property(n => n.Id)
                .IsRequired()
                .ValueGeneratedNever();

            builder.Property(n => n.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(n => n.ActivityType)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(n => n.ActivityPropertiesJson)
                .IsRequired();

            builder.Property(n => n.InputMappingsJson)
                .IsRequired();

            builder.Property(n => n.OutputMappingsJson)
                .IsRequired();

            builder.Property(n => n.PositionJson)
                .IsRequired();

            // Relationships
            builder.HasOne(n => n.Workflow)
                .WithMany(w => w.Nodes)
                .HasForeignKey(n => n.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);            // Remove any shadow properties
            var navigation = builder.Metadata.FindNavigation(nameof(WorkflowNode.Workflow));
            if (navigation != null)
            {
                navigation.SetPropertyAccessMode(PropertyAccessMode.Property);
            }
        }
    }
}