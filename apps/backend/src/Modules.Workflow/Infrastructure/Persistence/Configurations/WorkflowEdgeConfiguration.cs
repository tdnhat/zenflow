using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Workflow.DDD.Entities;

namespace Modules.Workflow.Infrastructure.Persistence.Configurations
{
    public class WorkflowEdgeConfiguration : IEntityTypeConfiguration<WorkflowEdge>
    {
        public void Configure(EntityTypeBuilder<WorkflowEdge> builder)
        {
            builder.ToTable("WorkflowEdges");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.WorkflowId)
                .IsRequired();

            builder.Property(e => e.SourceNodeId)
                .IsRequired();

            builder.Property(e => e.TargetNodeId)
                .IsRequired();

            builder.Property(e => e.Label)
                .HasMaxLength(200);

            builder.Property(e => e.EdgeType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.ConditionJson)
                .HasColumnType("jsonb");

            builder.Property(e => e.SourceHandle)
                .HasMaxLength(100);

            builder.Property(e => e.TargetHandle)
                .HasMaxLength(100);

            // Configure the relationship to Workflow - fix to prevent shadow property
            builder.HasOne(e => e.Workflow)
                .WithMany(w => w.Edges)
                .HasForeignKey(e => e.WorkflowId)
                .HasConstraintName("FK_WorkflowEdges_Workflows_WorkflowId")
                .OnDelete(DeleteBehavior.Cascade);

            // Configure the relationship to source and target nodes with constraint names
            builder.HasOne<WorkflowNode>()
                .WithMany()
                .HasForeignKey(e => e.SourceNodeId)
                .HasConstraintName("FK_WorkflowEdges_WorkflowNodes_SourceNodeId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<WorkflowNode>()
                .WithMany()
                .HasForeignKey(e => e.TargetNodeId)
                .HasConstraintName("FK_WorkflowEdges_WorkflowNodes_TargetNodeId")
                .OnDelete(DeleteBehavior.Restrict);

            // Configure audit fields from Aggregate base class
            builder.Property(e => e.CreatedAt)
                .IsRequired();

            builder.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.LastModifiedAt);

            builder.Property(e => e.LastModifiedBy)
                .HasMaxLength(100);
        }
    }
}