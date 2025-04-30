using Modules.User.Dtos;

namespace Modules.User.DDD.Interfaces
{
    public interface IUserRepository
    {
        Task<List<DDD.Entities.User>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<DDD.Entities.User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<DDD.Entities.User?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);
        Task AddAsync(DDD.Entities.User user, CancellationToken cancellationToken = default);
        Task UpdateAsync(DDD.Entities.User user, CancellationToken cancellationToken = default);
    }
}
