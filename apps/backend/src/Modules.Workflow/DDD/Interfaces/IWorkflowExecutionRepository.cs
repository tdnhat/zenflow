using Modules.Workflow.Features.WorkflowExecutions.GetWorkflowExecutions;
using ZenFlow.Shared.Application.Models;

namespace Modules.Workflow.DDD.Interfaces
{
    public interface IWorkflowExecutionRepository
    {
        Task AddAsync(DDD.Entities.WorkflowExecution workflowExecution, CancellationToken cancellationToken = default);
        
        Task<IEnumerable<DDD.Entities.WorkflowExecution>> GetByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken = default);
        
        Task<IEnumerable<DDD.Entities.WorkflowExecution>> GetFilteredAsync(WorkflowExecutionsFilterRequest filter, CancellationToken cancellationToken = default);
        
        Task<DDD.Entities.WorkflowExecution?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        
        Task<DDD.Entities.WorkflowExecution?> GetByIdWithNodeExecutionsAsync(Guid id, CancellationToken cancellationToken = default);
        
        Task UpdateAsync(DDD.Entities.WorkflowExecution workflowExecution, CancellationToken cancellationToken = default);
    }
}