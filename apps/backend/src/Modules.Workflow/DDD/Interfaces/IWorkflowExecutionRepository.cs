using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Modules.Workflow.DDD.Interfaces
{
    /// <summary>
    /// Repository interface for workflow execution entities
    /// </summary>
    public interface IWorkflowExecutionRepository : IRepository<Entities.WorkflowExecution, Guid>
    {
        Task<List<Entities.WorkflowExecution>> GetByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken = default);
        Task<int> CountByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken = default);
    }
}