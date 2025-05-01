using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using ZenFlow.Shared.Domain;

namespace Modules.Workflow.Data
{
    public class WorkflowDbContext : DbContext
    {
        public WorkflowDbContext(DbContextOptions<WorkflowDbContext> options)
            : base(options)
        {
        }

        public DbSet<DDD.Entities.Workflow> Workflows => Set<DDD.Entities.Workflow>();
        public DbSet<DDD.Entities.WorkflowNode> WorkflowNodes => Set<DDD.Entities.WorkflowNode>();
        public DbSet<DDD.Entities.WorkflowEdge> WorkflowEdges => Set<DDD.Entities.WorkflowEdge>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("workflow");

            // Configure the entity mappings
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(WorkflowDbContext).Assembly);

            // Apply global filters for soft delete
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "p");
                    var propertyMethodInfo = typeof(EF).GetMethod(nameof(EF.Property))!
                        .MakeGenericMethod(typeof(bool));
                    var isDeletedProperty = Expression.Call(propertyMethodInfo,
                        parameter,
                        Expression.Constant(nameof(ISoftDelete.IsDeleted)));

                    var notDeletedExpression = Expression.Not(isDeletedProperty);
                    var lambda = Expression.Lambda(notDeletedExpression, parameter);

                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }

            base.OnModelCreating(modelBuilder);
        }
    }
}
