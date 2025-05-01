namespace Modules.Workflow.DDD.Interfaces
{
    public interface INodeExecutionRepository
    {
        Task AddAsync(DDD.Entities.NodeExecution nodeExecution, CancellationToken cancellationToken = default);
        
        Task<IEnumerable<DDD.Entities.NodeExecution>> GetByWorkflowExecutionIdAsync(Guid workflowExecutionId, CancellationToken cancellationToken = default);
        
        Task<DDD.Entities.NodeExecution?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        
        Task<DDD.Entities.NodeExecution?> GetByNodeIdAndExecutionIdAsync(Guid nodeId, Guid workflowExecutionId, CancellationToken cancellationToken = default);
        
        Task UpdateAsync(DDD.Entities.NodeExecution nodeExecution, CancellationToken cancellationToken = default);
        
        Task AddRangeAsync(IEnumerable<DDD.Entities.NodeExecution> nodeExecutions, CancellationToken cancellationToken = default);
    }
}