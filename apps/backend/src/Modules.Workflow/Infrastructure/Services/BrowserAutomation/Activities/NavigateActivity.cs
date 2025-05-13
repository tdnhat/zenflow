using Elsa.Extensions;
using Elsa.Workflows;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Modules.Workflow.DDD.Interfaces;

namespace Modules.Workflow.Infrastructure.Services.BrowserAutomation.Activities
{
    /// <summary>
    /// Navigates the browser to a specified URL
    /// </summary>
    public class NavigateActivity : CodeActivity
    {
        private readonly IBrowserSessionManager _browserSessionManager;
        private readonly ILogger<NavigateActivity> _logger;

        /// <summary>
        /// Constructor for the NavigateActivity
        /// </summary>
        public NavigateActivity(
            IBrowserSessionManager browserSessionManager,
            ILogger<NavigateActivity> logger)
        {
            _browserSessionManager = browserSessionManager;
            _logger = logger;
        }

        /// <summary>
        /// The URL to navigate to
        /// </summary>
        public string Url { get; set; } = default!;

        /// <summary>
        /// The navigation timeout in milliseconds
        /// </summary>
        public int Timeout { get; set; } = 60000;

        /// <summary>
        /// When to consider navigation succeeded
        /// </summary>
        public string WaitUntil { get; set; } = "load";

        /// <summary>
        /// Executes the activity
        /// </summary>
        protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var workflowInstanceId = context.WorkflowExecutionContext.Id.ToString();
            _logger.LogInformation("Executing NavigateActivity for workflow {WorkflowInstanceId} to URL {Url}",
                workflowInstanceId, Url);

            var page = await _browserSessionManager.GetOrCreatePageForWorkflowAsync(workflowInstanceId, context.CancellationToken);
            var url = Url;

            // Add http:// if not present
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                _logger.LogDebug("URL does not contain protocol, prefixing with https://");
                url = "https://" + url;
            }

            // Configure navigation options
            var waitUntilState = WaitUntil switch
            {
                "domcontentloaded" => WaitUntilState.DOMContentLoaded,
                "networkidle" => WaitUntilState.NetworkIdle,
                _ => WaitUntilState.Load
            };

            _logger.LogDebug("Navigating to {Url} with timeout {Timeout}ms and waitUntil {WaitUntil}",
                url, Timeout, waitUntilState);

            try
            {
                // Navigate to the URL
                var response = await page.GotoAsync(url, new()
                {
                    Timeout = Timeout,
                    WaitUntil = waitUntilState
                });

                if (response != null)
                {
                    var responseUrl = response.Url;
                    var statusCode = response.Status;
                    var pageTitle = await page.TitleAsync();

                    _logger.LogInformation("Navigation successful: Status {StatusCode}, Title: {PageTitle}",
                        statusCode, pageTitle);

                    // Store variables for access in other activities
                    context.SetVariable("PageUrl", responseUrl);
                    context.SetVariable("PageTitle", pageTitle);
                    context.SetVariable("LastStatusCode", statusCode);
                }
                else
                {
                    _logger.LogWarning("Navigation completed but no response was returned");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Navigation to {Url} failed: {ErrorMessage}", url, ex.Message);
                throw new Exception($"Navigation operation failed: {ex.Message}", ex);
            }
        }
    }
}
