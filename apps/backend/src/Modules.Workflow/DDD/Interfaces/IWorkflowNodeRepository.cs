using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Workflow.DDD.Interfaces
{
    public interface IWorkflowNodeRepository : IRepository<DDD.Entities.WorkflowNode, Guid>
    {
        Task<IEnumerable<DDD.Entities.WorkflowNode>> GetByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken = default);
        Task DeleteAsync(DDD.Entities.WorkflowNode node, CancellationToken cancellationToken = default); // Overload for entity-based deletion
    }
}