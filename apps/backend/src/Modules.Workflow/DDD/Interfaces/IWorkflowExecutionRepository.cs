using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Modules.Workflow.DDD.Interfaces
{
    /// <summary>
    /// Repository interface for workflow execution entities
    /// </summary>
    public interface IWorkflowExecutionRepository
    {
        Task<Entities.WorkflowExecution?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<Entities.WorkflowExecution>> GetAllAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<List<Entities.WorkflowExecution>> GetByWorkflowIdAsync(Guid workflowId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
        Task<int> CountAsync(CancellationToken cancellationToken = default);
        Task<int> CountByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken = default);
        Task AddAsync(Entities.WorkflowExecution execution, CancellationToken cancellationToken = default);
        Task UpdateAsync(Entities.WorkflowExecution execution, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}