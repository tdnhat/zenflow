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
            
            builder.Property(i => i.WorkflowDefinitionId)
                .IsRequired();
                
            builder.Property(i => i.Status)
                .IsRequired()
                .HasConversion<string>();
                
            builder.Property(i => i.StartedAt);
            
            builder.Property(i => i.CompletedAt);
            
            builder.Property(i => i.Error)
                .HasMaxLength(4000);
                
            builder.Property(i => i.VariablesJson)
                .HasColumnType("nvarchar(max)");
        }
    }
}