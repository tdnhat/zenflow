using Modules.Workflow.Features.WorkflowEdges.UpdateEdge;
using Modules.Workflow.Features.WorkflowNodes.UpdateNode;
using Modules.Workflow.Features.Workflows.Common;

namespace Modules.Workflow.DDD.Interfaces
{
    /// <summary>
    /// Repository interface for workflow entities
    /// </summary>
    public interface IWorkflowRepository : IRepository<Entities.Workflow, Guid>
    {
        Task<Entities.Workflow?> GetByIdWithNodesAndEdgesAsync(Guid id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Forces an update of workflow nodes and edges, bypassing EF Core's optimistic concurrency checks.
        /// Used as a fallback when regular updates consistently fail due to concurrency conflicts.
        /// </summary>
        /// <param name="workflowId">The ID of the workflow to update</param>
        /// <param name="nodes">The new collection of nodes</param>
        /// <param name="edges">The new collection of edges</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task ForceUpdateNodesAndEdgesAsync(
            Guid workflowId, 
            List<UpdateWorkflowNodeRequest> nodes, 
            List<UpdateWorkflowEdgeRequest> edges, 
            CancellationToken cancellationToken = default);

        Task<IEnumerable<Entities.Workflow>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    }
}