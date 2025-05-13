using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Modules.Workflow.DDD.Entities;
using Modules.Workflow.DDD.ValueObjects;

namespace Modules.Workflow.Infrastructure.Persistence.Configurations
{
    public class WorkflowExecutionConfiguration : IEntityTypeConfiguration<WorkflowExecution>
    {
        public void Configure(EntityTypeBuilder<WorkflowExecution> builder)
        {
            builder.ToTable("WorkflowExecutions");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.WorkflowId)
                .IsRequired();

            builder.Property(e => e.WorkflowVersion)
                .IsRequired();

            // Configure Status enum to be stored as a string in the database    
            builder.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasConversion(
                    v => v.ToStringValue(),
                    v => WorkflowExecutionStatusExtensions.FromString(v));

            builder.Property(e => e.StartedAt)
                .IsRequired();

            builder.Property(e => e.CompletedAt);

            builder.Property(e => e.ErrorMessage)
                .HasMaxLength(1000);

            builder.Property(e => e.ErrorStack);

            builder.Property(e => e.ErrorNodeId);

            // Configure the relationship to Workflow - fix to prevent shadow property creation
            builder.HasOne(e => e.Workflow)
                .WithMany()
                .HasForeignKey(e => e.WorkflowId)
                .HasConstraintName("FK_WorkflowExecutions_Workflows_WorkflowId")
                .OnDelete(DeleteBehavior.Restrict);

            // Configure the relationship to NodeExecutions
            builder.HasMany(e => e.NodeExecutions)
                .WithOne(n => n.WorkflowExecution)
                .HasForeignKey(n => n.WorkflowExecutionId)
                .OnDelete(DeleteBehavior.Cascade);

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