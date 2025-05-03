using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Workflow.DDD.Entities;

namespace Modules.Workflow.Data.Configurations
{
    public class WorkflowNodeConfiguration : IEntityTypeConfiguration<WorkflowNode>
    {
        public void Configure(EntityTypeBuilder<WorkflowNode> builder)
        {
            builder.ToTable("WorkflowNodes");
            
            builder.HasKey(n => n.Id);
            
            builder.Property(n => n.WorkflowId)
                .IsRequired();
                
            builder.Property(n => n.NodeType)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(n => n.NodeKind)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(n => n.Label)
                .HasMaxLength(200);
                
            builder.Property(n => n.X)
                .IsRequired();
                
            builder.Property(n => n.Y)
                .IsRequired();
                
            builder.Property(n => n.ConfigJson)
                .HasColumnType("jsonb");
                
            // Configure the relationship to Workflow - fix to prevent shadow property
            builder.HasOne(n => n.Workflow)
                .WithMany(w => w.Nodes)
                .HasForeignKey(n => n.WorkflowId)
                .HasConstraintName("FK_WorkflowNodes_Workflows_WorkflowId")
                .OnDelete(DeleteBehavior.Cascade);
                
            // Configure audit fields from Aggregate base class
            builder.Property(n => n.CreatedAt)
                .IsRequired();
                
            builder.Property(n => n.CreatedBy)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(n => n.LastModifiedAt);
            
            builder.Property(n => n.LastModifiedBy)
                .HasMaxLength(100);
        }
    }
}