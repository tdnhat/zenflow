using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Workflow.DDD.Interfaces
{
    public interface IWorkflowEdgeRepository : IRepository<DDD.Entities.WorkflowEdge, Guid>
    {
        Task<IEnumerable<DDD.Entities.WorkflowEdge>> GetByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken = default);
        Task DeleteAsync(DDD.Entities.WorkflowEdge edge, CancellationToken cancellationToken = default); // Overload for entity-based deletion
    }
}