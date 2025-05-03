using Microsoft.EntityFrameworkCore;
using Modules.Workflow.Data;
using Modules.Workflow.DDD.Interfaces;
using Modules.Workflow.Infrastructure.Events;
using ZenFlow.Shared.Domain;

namespace Modules.Workflow.Repositories
{
    /// <summary>
    /// Base repository implementation for entity types
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <typeparam name="TKey">Entity key type</typeparam>
    public class Repository<TEntity, TKey> : IRepository<TEntity, TKey> 
        where TEntity : Entity<TKey>
        where TKey : IEquatable<TKey>
    {
        protected readonly WorkflowDbContext DbContext;
        protected readonly DbSet<TEntity> DbSet;
        protected readonly IWorkflowDomainEventService? DomainEventService;

        public Repository(WorkflowDbContext dbContext, IWorkflowDomainEventService? domainEventService = null)
        {
            DbContext = dbContext;
            DbSet = dbContext.Set<TEntity>();
            DomainEventService = domainEventService;
        }

        public async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
        {
            return await DbSet.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<List<TEntity>> GetAllAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await DbSet.CountAsync(cancellationToken);
        }

        public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await DbSet.AddAsync(entity, cancellationToken);
        }

        public Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            DbSet.Update(entity);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(TKey id, CancellationToken cancellationToken = default)
        {
            var entity = await GetByIdAsync(id, cancellationToken);
            if (entity != null)
            {
                DbSet.Remove(entity);
            }
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (DomainEventService != null)
            {
                await DispatchDomainEventsAsync();
            }
            
            await DbContext.SaveChangesAsync(cancellationToken);
        }

        protected async Task DispatchDomainEventsAsync()
        {
            if (DomainEventService == null)
                return;
                
            // Get all entities with domain events
            var entitiesWithEvents = DbContext.ChangeTracker.Entries<Aggregate<TKey>>()
                .Select(e => e.Entity)
                .Where(e => e.DomainEvents.Any())
                .ToList();

            // Get all domain events
            var domainEvents = entitiesWithEvents
                .SelectMany(e => e.DomainEvents)
                .ToList();

            // Clear domain events from entities
            entitiesWithEvents.ForEach(e => e.ClearDomainEvents());

            // Dispatch domain events
            foreach (var domainEvent in domainEvents)
            {
                await DomainEventService.PublishAsync(domainEvent);
            }
        }
    }
}