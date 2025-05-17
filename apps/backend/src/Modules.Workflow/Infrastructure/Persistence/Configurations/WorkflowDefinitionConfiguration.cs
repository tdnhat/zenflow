using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Workflow.Domain.Entities;

namespace Modules.Workflow.Infrastructure.Persistence.Configurations
{
    public class WorkflowDefinitionConfiguration : IEntityTypeConfiguration<WorkflowDefinition>
    {
        public void Configure(EntityTypeBuilder<WorkflowDefinition> builder)
        {
            builder.ToTable("WorkflowDefinitions");
            
            builder.HasKey(w => w.Id);
            
            builder.Property(w => w.Name)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(w => w.Description)
                .HasMaxLength(500);
                
            builder.Property(w => w.Version)
                .IsRequired();
                
            builder.Property(w => w.CreatedAt)
                .IsRequired();
                
            builder.Property(w => w.UpdatedAt);
            
            // Configure relationships
            builder.HasMany(w => w.Nodes)
                .WithOne()
                .HasForeignKey(n => n.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.HasMany(w => w.Edges)
                .WithOne()
                .HasForeignKey(e => e.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}