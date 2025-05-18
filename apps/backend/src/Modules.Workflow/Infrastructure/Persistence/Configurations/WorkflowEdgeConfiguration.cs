using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Workflow.Domain.Entities;

namespace Modules.Workflow.Infrastructure.Persistence.Configurations
{
    public class WorkflowEdgeConfiguration : IEntityTypeConfiguration<WorkflowEdge>
    {
        public void Configure(EntityTypeBuilder<WorkflowEdge> builder)
        {
            builder.ToTable("WorkflowEdges");
            
            builder.HasKey(e => new { e.Id, e.WorkflowId });
            
            builder.Property(e => e.Id)
                .IsRequired()
                .ValueGeneratedNever();
                
            builder.Property(e => e.Source)
                .IsRequired();
                
            builder.Property(e => e.Target)
                .IsRequired();
                
            builder.Property(e => e.ConditionJson);
                
            // Relationships
            builder.HasOne(e => e.Workflow)
                .WithMany(w => w.Edges)
                .HasForeignKey(e => e.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure node source/target relationships properly
            builder.HasOne<WorkflowNode>()
                .WithMany()
                .HasPrincipalKey(n => new { n.Id, n.WorkflowId })
                .HasForeignKey(e => new { e.Source, e.WorkflowId })
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<WorkflowNode>()
                .WithMany()
                .HasPrincipalKey(n => new { n.Id, n.WorkflowId })
                .HasForeignKey(e => new { e.Target, e.WorkflowId })
                .OnDelete(DeleteBehavior.Cascade);
            
            // Remove any shadow properties
            var navigation = builder.Metadata.FindNavigation(nameof(WorkflowEdge.Workflow));
            if (navigation != null)
            {
                navigation.SetPropertyAccessMode(PropertyAccessMode.Property);
            }
        }
    }
}