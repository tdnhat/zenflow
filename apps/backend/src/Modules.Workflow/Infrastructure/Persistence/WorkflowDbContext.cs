using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Modules.Workflow.Domain.Entities;

namespace Modules.Workflow.Infrastructure.Persistence
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

        public DbSet<WorkflowDefinition> WorkflowDefinitions { get; set; }
        public DbSet<WorkflowNode> WorkflowNodes { get; set; }
        public DbSet<WorkflowEdge> WorkflowEdges { get; set; }
        public DbSet<WorkflowInstance> WorkflowInstances { get; set; }
        public DbSet<NodeExecution> NodeExecutions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Create the schema first
            modelBuilder.HasPostgresExtension("uuid-ossp");
            modelBuilder.HasDefaultSchema("workflow");

            // Configure the entity mappings
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(WorkflowDbContext).Assembly);

            base.OnModelCreating(modelBuilder);

            // Apply global filters for soft delete
            // foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            // {
            //     if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            //     {
            //         var parameter = Expression.Parameter(entityType.ClrType, "p");
            //         var propertyMethodInfo = typeof(EF).GetMethod(nameof(EF.Property))!
            //             .MakeGenericMethod(typeof(bool));
            //         var isDeletedProperty = Expression.Call(propertyMethodInfo,
            //             parameter,
            //             Expression.Constant(nameof(ISoftDelete.IsDeleted)));

            //         var notDeletedExpression = Expression.Not(isDeletedProperty);
            //         var lambda = Expression.Lambda(notDeletedExpression, parameter);

            //         modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            //     }
            // }
        }
    }
}
