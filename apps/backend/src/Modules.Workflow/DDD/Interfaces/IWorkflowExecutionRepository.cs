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
        
        /// <summary>
        /// Gets the most recent active execution (running or pending) for a workflow
        /// </summary>
        /// <param name="workflowId">The workflow ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The most recent active execution, or null if none exists</returns>
        Task<Entities.WorkflowExecution?> GetMostRecentActiveExecutionForWorkflowAsync(Guid workflowId, CancellationToken cancellationToken = default);
    }
}