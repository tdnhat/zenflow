using Microsoft.EntityFrameworkCore;
using Modules.Workflow.DDD.Interfaces;
using ZenFlow.Shared.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Modules.Workflow.Infrastructure.EventHandling.DomainEvents;
using Modules.Workflow.Infrastructure.Persistence;

namespace Modules.Workflow.Infrastructure.Persistence.Repositories
{
    public class WorkflowExecutionRepository : IWorkflowExecutionRepository
    {
        private readonly WorkflowDbContext _dbContext;
        private readonly IWorkflowDomainEventService _domainEventService;

        public WorkflowExecutionRepository(WorkflowDbContext dbContext, IWorkflowDomainEventService domainEventService)
        {
            _dbContext = dbContext;
            _domainEventService = domainEventService;
        }

        public async Task<DDD.Entities.WorkflowExecution?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.WorkflowExecutions
                .Include(e => e.NodeExecutions)
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public async Task<List<DDD.Entities.WorkflowExecution>> GetAllAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        {
            return await _dbContext.WorkflowExecutions
                .OrderByDescending(e => e.StartedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<DDD.Entities.WorkflowExecution>> GetByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.WorkflowExecutions
                .Where(e => e.WorkflowId == workflowId)
                .OrderByDescending(e => e.StartedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.WorkflowExecutions.CountAsync(cancellationToken);
        }

        public async Task<int> CountByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.WorkflowExecutions
                .Where(e => e.WorkflowId == workflowId)
                .CountAsync(cancellationToken);
        }

        public async Task AddAsync(DDD.Entities.WorkflowExecution execution, CancellationToken cancellationToken = default)
        {
            await _dbContext.WorkflowExecutions.AddAsync(execution, cancellationToken);
        }

        public Task UpdateAsync(DDD.Entities.WorkflowExecution execution, CancellationToken cancellationToken = default)
        {
            _dbContext.WorkflowExecutions.Update(execution);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var execution = await GetByIdAsync(id, cancellationToken);
            if (execution != null)
            {
                _dbContext.WorkflowExecutions.Remove(execution);
            }
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var domainEvents = CollectDomainEvents();
            // Process domain events - this will add them to the DbContext but not save yet
            if (domainEvents.Any())
            {
                foreach (var domainEvent in domainEvents)
                {
                    await _domainEventService.PublishAsync(domainEvent);
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<DDD.Entities.WorkflowExecution?> GetMostRecentActiveExecutionForWorkflowAsync(Guid workflowId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.WorkflowExecutions
                .Where(e => e.WorkflowId == workflowId &&
                          (e.Status == DDD.ValueObjects.WorkflowExecutionStatus.RUNNING ||
                           e.Status == DDD.ValueObjects.WorkflowExecutionStatus.PENDING))
                .OrderByDescending(e => e.StartedAt)
                .FirstOrDefaultAsync(cancellationToken);
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

        private async Task DispatchDomainEventsAsync()
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

            // Dispatch domain events
            foreach (var domainEvent in domainEvents)
            {
                await _domainEventService.PublishAsync(domainEvent);
            }
        }
    }
}