using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ZenFlow.Shared.Domain;

namespace Modules.Workflow.DDD.Interfaces
{
    /// <summary>
    /// Generic repository interface for entity operations
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <typeparam name="TKey">The entity key type</typeparam>
    public interface IRepository<TEntity, TKey> where TEntity : Entity<TKey>
    {
        Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
        Task<List<TEntity>> GetAllAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<int> CountAsync(CancellationToken cancellationToken = default);
        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(TKey id, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}