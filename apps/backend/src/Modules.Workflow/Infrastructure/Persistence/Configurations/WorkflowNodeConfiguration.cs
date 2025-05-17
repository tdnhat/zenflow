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
            
            builder.HasKey(n => new { n.Id, n.WorkflowId });
            
            builder.Property(n => n.Id)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(n => n.Name)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(n => n.ActivityType)
                .IsRequired()
                .HasMaxLength(200);
                
            builder.Property(n => n.ActivityPropertiesJson)
                .HasColumnType("nvarchar(max)");
                
            builder.Property(n => n.InputMappingsJson)
                .HasColumnType("nvarchar(max)");
                
            builder.Property(n => n.OutputMappingsJson)
                .HasColumnType("nvarchar(max)");
                
            builder.Property(n => n.PositionJson)
                .HasColumnType("nvarchar(max)");
                
            // Ignore the non-persisted properties
            builder.Ignore(n => n.ActivityProperties);
            builder.Ignore(n => n.InputMappings);
            builder.Ignore(n => n.OutputMappings);
            builder.Ignore(n => n.Position);
        }
    }
}