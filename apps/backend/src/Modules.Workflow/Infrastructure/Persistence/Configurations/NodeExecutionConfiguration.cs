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
            
            builder.HasKey(n => new { n.NodeId, n.WorkflowInstanceId });
            
            builder.Property(n => n.NodeId)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(n => n.ActivityType)
                .IsRequired()
                .HasMaxLength(200);
                
            builder.Property(n => n.Status)
                .IsRequired()
                .HasConversion<string>();
                
            builder.Property(n => n.StartedAt);
            
            builder.Property(n => n.CompletedAt);
            
            builder.Property(n => n.Error)
                .HasMaxLength(4000);
                
            builder.Property(n => n.InputDataJson)
                .HasColumnType("nvarchar(max)");
                
            builder.Property(n => n.OutputDataJson)
                .HasColumnType("nvarchar(max)");
                
            builder.Property(n => n.LogsJson)
                .HasColumnType("nvarchar(max)");
            
            // Configure relationships
            builder.HasOne<WorkflowInstance>()
                .WithMany(i => i.NodeExecutions)
                .HasForeignKey(n => n.WorkflowInstanceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}