using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Workflow.Domain.Entities;

namespace Modules.Workflow.Infrastructure.Persistence.Configurations
{
    public class NodeExecutionConfiguration : IEntityTypeConfiguration<NodeExecution>
    {
        public void Configure(EntityTypeBuilder<NodeExecution> builder)
        {
            builder.ToTable("NodeExecutions");

            // Primary key
            builder.HasKey(ne => new { ne.NodeId, ne.WorkflowInstanceId });

            // Properties
            builder.Property(ne => ne.NodeId)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(ne => ne.ActivityType)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(ne => ne.Status)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(ne => ne.Error)
                .IsRequired()
                .HasMaxLength(4000);

            builder.Property(ne => ne.InputDataJson)
                .IsRequired();

            builder.Property(ne => ne.OutputDataJson)
                .IsRequired();

            builder.Property(ne => ne.LogsJson)
                .IsRequired();

            // Relationships - Fix the relationship to use proper property
            builder.HasOne(ne => ne.WorkflowInstance)
                .WithMany(wi => wi.NodeExecutions)
                .HasForeignKey(ne => ne.WorkflowInstanceId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Remove any shadow properties
            builder.Metadata.FindNavigation(nameof(NodeExecution.WorkflowInstance))
                .SetPropertyAccessMode(PropertyAccessMode.Property);
        }
    }
}