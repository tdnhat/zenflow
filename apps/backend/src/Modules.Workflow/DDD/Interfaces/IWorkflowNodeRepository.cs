namespace Modules.Workflow.DDD.Interfaces
{
    public interface IWorkflowNodeRepository
    {
        Task AddAsync(DDD.Entities.WorkflowNode node, CancellationToken cancellationToken = default);
        
        Task<DDD.Entities.WorkflowNode?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        
        Task<IEnumerable<DDD.Entities.WorkflowNode>> GetByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken = default);
        
        Task UpdateAsync(DDD.Entities.WorkflowNode node, CancellationToken cancellationToken = default);
        
        Task DeleteAsync(DDD.Entities.WorkflowNode node, CancellationToken cancellationToken = default);
    }
}