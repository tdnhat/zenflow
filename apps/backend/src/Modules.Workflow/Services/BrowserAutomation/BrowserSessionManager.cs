using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Modules.Workflow.DDD.Interfaces;
using System.Collections.Concurrent;

namespace Modules.Workflow.Services.BrowserAutomation
{
    public class BrowserSessionManager : IBrowserSessionManager, IAsyncDisposable
    {
        private readonly ConcurrentDictionary<string, IBrowser> _browsers = new();
        private readonly ConcurrentDictionary<string, IPage> _pages = new();
        private readonly SemaphoreSlim _semaphore = new(1);
        private readonly IBrowserAutomation _browserAutomation;
        private readonly ILogger<BrowserSessionManager> _logger;

        public BrowserSessionManager(IBrowserAutomation browserAutomation, ILogger<BrowserSessionManager> logger)
        {
            _browserAutomation = browserAutomation;
            _logger = logger;
        }

        /// <summary>
        /// Cleans up all resources for a workflow.
        /// </summary>
        /// <param name="workflowInstanceId">The ID of the workflow instance.</param>
        public async Task CleanupWorkflowResourcesAsync(string workflowInstanceId)
        {
            await _semaphore.WaitAsync();
            try
            {
                // Close page if it exists
                if (_pages.TryGetValue(workflowInstanceId, out var page))
                {
                    await page.CloseAsync();
                    _pages.TryRemove(workflowInstanceId, out _);
                }

                // Close and dispose browser
                if (_browsers.TryGetValue(workflowInstanceId, out var browser))
                {
                    await browser.CloseAsync();
                    _browsers.TryRemove(workflowInstanceId, out _);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Disposes of all browser and page resources.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            foreach (var pageKvp in _pages.ToList())
            {
                await CleanupWorkflowResourcesAsync(pageKvp.Key);
            }
        }

        /// <summary>
        /// Gets or creates a page for a workflow.
        /// </summary>
        /// <param name="workflowInstanceId">The ID of the workflow instance.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The page for the workflow.</returns>
        public async Task<IPage> GetOrCreatePageForWorkflowAsync(string workflowInstanceId, CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                // Check if page already exists
                if (_pages.TryGetValue(workflowInstanceId, out var existingPage))
                {
                    return existingPage;
                }

                // Create new browser session if needed
                if (!_browsers.TryGetValue(workflowInstanceId, out var browser))
                {
                    browser = await _browserAutomation.LaunchBrowserAsync(
                        new BrowserLaunchOptions { Headless = true }
                    );
                    _browsers[workflowInstanceId] = browser;
                }

                // Create new page
                var page = await browser.NewPageAsync();
                _pages[workflowInstanceId] = page;

                return page;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Closes the browser session for a workflow
        /// </summary>
        /// <param name="workflowInstanceId">The ID of the workflow instance</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task CloseSessionAsync(string workflowInstanceId, CancellationToken cancellationToken = default)
        {
            // We'll reuse the existing cleanup method
            _logger.LogInformation("Closing browser session for workflow {WorkflowInstanceId}", workflowInstanceId);
            await CleanupWorkflowResourcesAsync(workflowInstanceId);
        }
    }
}
