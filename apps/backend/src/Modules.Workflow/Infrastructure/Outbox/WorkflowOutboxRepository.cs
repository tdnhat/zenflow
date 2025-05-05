using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Workflow.Data;
using Modules.Workflow.DDD.Entities;
using Modules.Workflow.DDD.Interfaces;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using ZenFlow.Shared.Domain;

namespace Modules.Workflow.Infrastructure.Outbox
{
    public class WorkflowOutboxRepository : IWorkflowOutboxRepository
    {
        private readonly WorkflowDbContext _context;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public WorkflowOutboxRepository(WorkflowDbContext context, IServiceScopeFactory serviceScopeFactory)
        {
            _context = context;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task AddAsync(WorkflowOutboxMessage message)
        {
            await _context.OutboxMessages.AddAsync(message);
            await _context.SaveChangesAsync();
        }

        public async Task StoreAsync(WorkflowOutboxMessage message)
        {
            await _context.OutboxMessages.AddAsync(message);
        }

        public async Task AddInSeparateTransactionAsync(WorkflowOutboxMessage message)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var newContext = scope.ServiceProvider.GetRequiredService<WorkflowDbContext>();

            var strategy = newContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await newContext.Database.BeginTransactionAsync();
                try
                {
                    await newContext.OutboxMessages.AddAsync(message);
                    await newContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    throw;
                }
            });
        }

        public async Task UpdateAsync(WorkflowOutboxMessage message)
        {
            _context.OutboxMessages.Update(message);
            await _context.SaveChangesAsync();
        }

        public async Task<List<WorkflowOutboxMessage>> GetUnprocessedMessagesAsync(int batchSize)
        {
            return await _context.OutboxMessages
                .Where(m => m.ProcessedOn == null)
                .OrderBy(m => m.OccurredOn)
                .Take(batchSize)
                .ToListAsync();
        }

        public async Task DeleteProcessedMessagesOlderThanAsync(DateTime date)
        {
            var oldMessages = await _context.OutboxMessages
                .Where(m => m.ProcessedOn != null && m.ProcessedOn < date)
                .ToListAsync();

            if (oldMessages.Any())
            {
                _context.OutboxMessages.RemoveRange(oldMessages);
                await _context.SaveChangesAsync();
            }
        }
    }

    public static class WorkflowOutboxExtensions
    {
        public static async Task PublishDomainEventAsync<T>(this IWorkflowOutboxRepository repository, T domainEvent) 
            where T : IDomainEvent
        {
            var message = new WorkflowOutboxMessage
            {
                Id = Guid.NewGuid(),
                EventType = domainEvent.GetType().AssemblyQualifiedName,
                EventContent = JsonSerializer.Serialize(domainEvent),
                OccurredOn = DateTime.UtcNow
            };

            await repository.AddAsync(message);
        }
    }
}