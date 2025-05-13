using Microsoft.EntityFrameworkCore;
using Modules.User.Domain.Entities;
using System.Linq.Expressions;
using ZenFlow.Shared.Domain;

namespace Modules.User.Infrastructure.Persistence
{


    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {
        }

        public DbSet<Domain.Entities.User> Users => Set<Domain.Entities.User>();
        public DbSet<UserOutboxMessage> OutboxMessages => Set<UserOutboxMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("user");

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserDbContext).Assembly);

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
