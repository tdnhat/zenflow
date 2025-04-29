namespace Modules.User.DDD.Interfaces
{
    public interface IUserRepository
    {
        Task<DDD.Entities.User?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);
        Task AddAsync(DDD.Entities.User user, CancellationToken cancellationToken = default);
    }
}
