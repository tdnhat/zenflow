namespace Modules.Workflow.DDD.Interfaces
{
    public interface IWorkflowEdgeRepository
    {
        Task AddAsync(DDD.Entities.WorkflowEdge edge, CancellationToken cancellationToken = default);
        
        Task<DDD.Entities.WorkflowEdge?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        
        Task<IEnumerable<DDD.Entities.WorkflowEdge>> GetByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken = default);
        
        Task UpdateAsync(DDD.Entities.WorkflowEdge edge, CancellationToken cancellationToken = default);
        
        Task DeleteAsync(DDD.Entities.WorkflowEdge edge, CancellationToken cancellationToken = default);
    }
}