using Modules.User.DDD.Entities;

namespace Modules.User.DDD.Interfaces
{
    public interface IUserOutboxRepository
    {
        Task AddAsync(UserOutboxMessage message);
        Task UpdateAsync(UserOutboxMessage message);
        Task<List<UserOutboxMessage>> GetUnprocessedMessagesAsync(int batchSize);
        Task DeleteProcessedMessagesOlderThanAsync(DateTime date);
    }
}
