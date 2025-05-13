using Modules.User.Domain.Entities;

namespace Modules.User.Domain.Interfaces
{
    public interface IUserOutboxRepository
    {
        Task AddAsync(UserOutboxMessage message);
        Task UpdateAsync(UserOutboxMessage message);
        Task<List<UserOutboxMessage>> GetUnprocessedMessagesAsync(int batchSize);
        Task DeleteProcessedMessagesOlderThanAsync(DateTime date);
    }
}
