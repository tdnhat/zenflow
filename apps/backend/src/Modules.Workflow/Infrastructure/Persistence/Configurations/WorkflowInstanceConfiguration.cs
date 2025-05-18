using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Workflow.Domain.Entities;

namespace Modules.Workflow.Infrastructure.Persistence.Configurations
{
    public class WorkflowInstanceConfiguration : IEntityTypeConfiguration<WorkflowInstance>
    {
        public void Configure(EntityTypeBuilder<WorkflowInstance> builder)
        {
            builder.ToTable("WorkflowInstances");
            
            builder.HasKey(i => i.Id);
            
            builder.Property(i => i.Status)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(i => i.Error)
                .IsRequired()
                .HasMaxLength(4000);
                
            builder.Property(i => i.StartedAt);
            
            builder.Property(i => i.CompletedAt);
            
            builder.Property(i => i.VariablesJson)
                .IsRequired();

            builder.HasMany(i => i.NodeExecutions)
                .WithOne(n => n.WorkflowInstance)
                .HasForeignKey(n => n.WorkflowInstanceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(i => i.WorkflowDefinition)
                .WithMany()
                .HasForeignKey(i => i.WorkflowDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}