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
                .HasMaxLength(50);
                
            builder.Property(e => e.Source)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(e => e.Target)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(e => e.ConditionJson)
                .HasColumnType("nvarchar(max)");
                
            // Ignore the non-persisted property
            builder.Ignore(e => e.Condition);
        }
    }
}