using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Workflow.DDD.Interfaces
{
    /// <summary>
    /// Repository interface for workflow entities
    /// </summary>
    public interface IWorkflowRepository : IRepository<Entities.Workflow, Guid>
    {
        Task<Entities.Workflow?> GetByIdWithNodesAndEdgesAsync(Guid id, CancellationToken cancellationToken = default);
    }
}