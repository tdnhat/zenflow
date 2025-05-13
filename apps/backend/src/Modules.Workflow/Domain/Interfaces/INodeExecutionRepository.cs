using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Workflow.DDD.Interfaces
{
    public interface INodeExecutionRepository : IRepository<DDD.Entities.NodeExecution, Guid>
    {
        Task<IEnumerable<DDD.Entities.NodeExecution>> GetByWorkflowExecutionIdAsync(Guid workflowExecutionId, CancellationToken cancellationToken = default);
        
        Task<DDD.Entities.NodeExecution?> GetByNodeIdAndExecutionIdAsync(Guid nodeId, Guid workflowExecutionId, CancellationToken cancellationToken = default);
        
        Task AddRangeAsync(IEnumerable<DDD.Entities.NodeExecution> nodeExecutions, CancellationToken cancellationToken = default);
    }
}