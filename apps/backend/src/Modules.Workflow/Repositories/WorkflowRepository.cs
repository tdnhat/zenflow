using Microsoft.EntityFrameworkCore;
using Modules.Workflow.Data;
using Modules.Workflow.DDD.Interfaces;
using Modules.Workflow.Infrastructure.Events;
using ZenFlow.Shared.Domain;

namespace Modules.Workflow.Repositories
{
    public class WorkflowRepository : IWorkflowRepository
    {
        private readonly WorkflowDbContext _dbContext;
        private readonly IWorkflowDomainEventService _domainEventService;

        public WorkflowRepository(WorkflowDbContext dbContext, IWorkflowDomainEventService domainEventService)
        {
            _dbContext = dbContext;
            _domainEventService = domainEventService;
        }

        public async Task<DDD.Entities.Workflow?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Workflows.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<DDD.Entities.Workflow?> GetByIdWithNodesAndEdgesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Workflows
                .Include(w => w.Nodes)
                .Include(w => w.Edges)
                .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
        }

        public async Task<List<DDD.Entities.Workflow>> GetAllAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Workflows
                .OrderByDescending(w => w.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Workflows.CountAsync(cancellationToken);
        }

        public async Task AddAsync(DDD.Entities.Workflow workflow, CancellationToken cancellationToken = default)
        {
            await _dbContext.Workflows.AddAsync(workflow, cancellationToken);
        }

        public Task UpdateAsync(DDD.Entities.Workflow workflow, CancellationToken cancellationToken = default)
        {
            _dbContext.Workflows.Update(workflow);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var workflow = await GetByIdAsync(id, cancellationToken);
            if (workflow != null)
            {
                _dbContext.Workflows.Remove(workflow);
            }
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Collect domain events before saving
            var domainEvents = CollectDomainEvents();

            // Process domain events - this will add them to the DbContext but not save yet
            if (domainEvents.Any())
            {
                foreach (var domainEvent in domainEvents)
                {
                    await _domainEventService.PublishAsync(domainEvent);
                }
            }

            // Now save everything in a single transaction
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<DDD.Entities.Workflow>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Workflows
                .AsNoTracking()
                .Where(w => w.CreatedBy == userId)
                .ToListAsync(cancellationToken);
        }

        private List<IDomainEvent> CollectDomainEvents()
        {
            // Get all entities with domain events
            var entitiesWithEvents = _dbContext.ChangeTracker.Entries<Aggregate<Guid>>()
                .Select(e => e.Entity)
                .Where(e => e.DomainEvents.Any())
                .ToList();

            // Get all domain events
            var domainEvents = entitiesWithEvents
                .SelectMany(e => e.DomainEvents)
                .ToList();

            // Clear domain events from entities
            entitiesWithEvents.ForEach(e => e.ClearDomainEvents());

            return domainEvents;
        }
    }
}
