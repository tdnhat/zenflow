namespace Modules.Workflow.DDD.Interfaces
{
    public interface IWorkflowRepository
    {
        Task AddAsync(DDD.Entities.Workflow workflow, CancellationToken cancellationToken = default);
        
        Task<IEnumerable<DDD.Entities.Workflow>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        
        Task<DDD.Entities.Workflow?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        
        Task UpdateAsync(DDD.Entities.Workflow workflow, CancellationToken cancellationToken = default);
    }
}
