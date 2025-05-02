using Microsoft.Playwright;

namespace Modules.Workflow.DDD.Interfaces
{
    public interface IBrowserSessionManager : IAsyncDisposable
    {
        Task<IPage> GetOrCreatePageForWorkflowAsync(string workflowInstanceId, CancellationToken cancellationToken);
        Task CleanupWorkflowResourcesAsync(string workflowInstanceId);
    }
}
