using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Modules.Workflow.DDD.Interfaces
{
    /// <summary>
    /// Repository interface for workflow entities
    /// </summary>
    public interface IWorkflowRepository
    {
        Task<Entities.Workflow?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Entities.Workflow?> GetByIdWithNodesAndEdgesAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<Entities.Workflow>> GetAllAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<int> CountAsync(CancellationToken cancellationToken = default);
        Task AddAsync(Entities.Workflow workflow, CancellationToken cancellationToken = default);
        Task UpdateAsync(Entities.Workflow workflow, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}