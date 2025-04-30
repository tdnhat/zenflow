namespace Modules.User.DDD.Interfaces
{
    public interface IUserRepository
    {
        Task<List<DDD.Entities.User>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<List<DDD.Entities.User>> GetAllIncludingDeletedAsync(CancellationToken cancellationToken = default);
        Task<DDD.Entities.User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<DDD.Entities.User?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default);
        Task<DDD.Entities.User?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);
        Task AddAsync(DDD.Entities.User user, CancellationToken cancellationToken = default);
        Task UpdateAsync(DDD.Entities.User user, CancellationToken cancellationToken = default);
        Task DeleteAsync(DDD.Entities.User user, CancellationToken cancellationToken = default);
        Task PermanentlyDeleteAsync(DDD.Entities.User user, CancellationToken cancellationToken = default);
        Task RestoreAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
