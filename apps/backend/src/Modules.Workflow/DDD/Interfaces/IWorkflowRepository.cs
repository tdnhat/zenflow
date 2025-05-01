using Modules.Workflow.Features.GetWorkflows;
using ZenFlow.Shared.Application.Models;

namespace Modules.Workflow.DDD.Interfaces
{
    public interface IWorkflowRepository
    {
        Task AddAsync(DDD.Entities.Workflow workflow, CancellationToken cancellationToken = default);
        
        Task<IEnumerable<DDD.Entities.Workflow>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        
        Task<PaginatedResult<DDD.Entities.Workflow>> GetFilteredAsync(string userId, WorkflowsFilterRequest filter, CancellationToken cancellationToken = default);
        
        Task<DDD.Entities.Workflow?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        
        Task<DDD.Entities.Workflow?> GetByIdWithNodesAndEdgesAsync(Guid id, CancellationToken cancellationToken = default);
        
        Task UpdateAsync(DDD.Entities.Workflow workflow, CancellationToken cancellationToken = default);
    }
}
