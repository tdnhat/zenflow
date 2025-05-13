namespace Modules.User.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<List<Entities.User>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<List<Entities.User>> GetAllIncludingDeletedAsync(CancellationToken cancellationToken = default);
        Task<Entities.User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Entities.User?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Entities.User?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);
        Task AddAsync(Entities.User user, CancellationToken cancellationToken = default);
        Task UpdateAsync(Entities.User user, CancellationToken cancellationToken = default);
        Task DeleteAsync(Entities.User user, CancellationToken cancellationToken = default);
        Task PermanentlyDeleteAsync(Entities.User user, CancellationToken cancellationToken = default);
        Task RestoreAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
