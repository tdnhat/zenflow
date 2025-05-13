using Microsoft.EntityFrameworkCore;
using Modules.User.Domain.Entities;
using Modules.User.Domain.Interfaces;
using Modules.User.Infrastructure.Persistence;
using System.Text.Json;
using ZenFlow.Shared.Domain;

namespace Modules.User.Infrastructure.Persistence.Repositories
{
    public class UserOutboxRepository : IUserOutboxRepository
    {
        private readonly UserDbContext _context;

        public UserOutboxRepository(UserDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(UserOutboxMessage message)
        {
            await _context.OutboxMessages.AddAsync(message);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(UserOutboxMessage message)
        {
            _context.OutboxMessages.Update(message);
            await _context.SaveChangesAsync();
        }

        public async Task<List<UserOutboxMessage>> GetUnprocessedMessagesAsync(int batchSize)
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

    public static class UserOutboxExtensions
    {
        public static async Task PublishDomainEventAsync<T>(this IUserOutboxRepository repository, T domainEvent)
            where T : IDomainEvent
        {
            var message = new UserOutboxMessage
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