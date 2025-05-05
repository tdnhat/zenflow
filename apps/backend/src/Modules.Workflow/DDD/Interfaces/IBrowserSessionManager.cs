using Microsoft.Playwright;

namespace Modules.Workflow.DDD.Interfaces
{
    public interface IBrowserSessionManager : IAsyncDisposable
    {
        Task<IPage> GetOrCreatePageForWorkflowAsync(string workflowInstanceId, CancellationToken cancellationToken);
        Task CleanupWorkflowResourcesAsync(string workflowInstanceId);
        
        /// <summary>
        /// Closes the browser session for a workflow
        /// </summary>
        /// <param name="workflowInstanceId">The ID of the workflow instance</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task CloseSessionAsync(string workflowInstanceId, CancellationToken cancellationToken = default);
    }
}
