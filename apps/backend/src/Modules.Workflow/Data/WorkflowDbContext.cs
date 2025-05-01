using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.Linq.Expressions;
using ZenFlow.Shared.Domain;

namespace Modules.Workflow.Data
{
    // Factory for creating DbContext at design time (for migrations)
    public class WorkflowDbContextFactory : IDesignTimeDbContextFactory<WorkflowDbContext>
    {
        public WorkflowDbContext CreateDbContext(string[] args)
        {
            // Build configuration from the startup project
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true);

            var configuration = configBuilder.Build();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<WorkflowDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new WorkflowDbContext(optionsBuilder.Options);
        }
    }
    public class WorkflowDbContext : DbContext
    {
        public WorkflowDbContext(DbContextOptions<WorkflowDbContext> options)
            : base(options)
        {
        }

        public DbSet<DDD.Entities.Workflow> Workflows => Set<DDD.Entities.Workflow>();
        public DbSet<DDD.Entities.WorkflowNode> WorkflowNodes => Set<DDD.Entities.WorkflowNode>();
        public DbSet<DDD.Entities.WorkflowEdge> WorkflowEdges => Set<DDD.Entities.WorkflowEdge>();
        public DbSet<DDD.Entities.WorkflowExecution> WorkflowExecutions => Set<DDD.Entities.WorkflowExecution>();
        public DbSet<DDD.Entities.NodeExecution> NodeExecutions => Set<DDD.Entities.NodeExecution>();

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
