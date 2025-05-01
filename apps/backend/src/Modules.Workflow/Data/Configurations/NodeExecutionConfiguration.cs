using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Workflow.DDD.Entities;

namespace Modules.Workflow.Data.Configurations
{
    public class NodeExecutionConfiguration : IEntityTypeConfiguration<NodeExecution>
    {
        public void Configure(EntityTypeBuilder<NodeExecution> builder)
        {
            builder.ToTable("NodeExecutions");
            
            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.WorkflowExecutionId)
                .IsRequired();
                
            builder.Property(e => e.NodeId)
                .IsRequired();
                
            builder.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(20);
                
            builder.Property(e => e.StartedAt);
            
            builder.Property(e => e.CompletedAt);
            
            builder.Property(e => e.InputJson)
                .HasColumnType("jsonb");
                
            builder.Property(e => e.OutputJson)
                .HasColumnType("jsonb");
                
            builder.Property(e => e.Error);
            
            builder.Property(e => e.DurationMs);
            
            // Configure relationship to WorkflowExecution
            builder.HasOne(e => e.WorkflowExecution)
                .WithMany(w => w.NodeExecutions)
                .HasForeignKey(e => e.WorkflowExecutionId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Configure relationship to WorkflowNode
            builder.HasOne(e => e.Node)
                .WithMany()
                .HasForeignKey(e => e.NodeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}