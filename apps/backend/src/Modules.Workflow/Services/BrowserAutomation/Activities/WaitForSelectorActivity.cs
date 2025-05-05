using Elsa.Extensions;
using Elsa.Workflows;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Modules.Workflow.DDD.Interfaces;
using System;
using System.Threading.Tasks;

namespace Modules.Workflow.Services.BrowserAutomation.Activities
{
    /// <summary>
    /// An activity that waits for an element to appear in the DOM matching the specified CSS selector
    /// </summary>
    public class WaitForSelectorActivity : CodeActivity
    {
        private readonly IBrowserSessionManager _browserSessionManager;
        private readonly ILogger<WaitForSelectorActivity> _logger;

        /// <summary>
        /// Constructor for the WaitForSelectorActivity
        /// </summary>
        public WaitForSelectorActivity(
            IBrowserSessionManager browserSessionManager,
            ILogger<WaitForSelectorActivity> logger)
        {
            _browserSessionManager = browserSessionManager;
            _logger = logger;
        }

        /// <summary>
        /// The CSS selector to wait for
        /// </summary>
        public string Selector { get; set; } = default!;

        /// <summary>
        /// Timeout in milliseconds to wait for the selector to appear
        /// </summary>
        public int Timeout { get; set; } = 30000;

        /// <summary>
        /// The state to wait for: 'visible', 'hidden', 'attached', or 'detached'
        /// </summary>
        public string State { get; set; } = "visible";

        /// <summary>
        /// Executes the activity
        /// </summary>
        protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var workflowInstanceId = context.WorkflowExecutionContext.Id.ToString();
            var selector = Selector;
            var timeout = Timeout;
            
            _logger.LogInformation("Executing WaitForSelectorActivity for workflow {WorkflowInstanceId} with selector {Selector}", 
                workflowInstanceId, selector);

            try
            {
                // Get or create the browser page for this workflow
                var page = await _browserSessionManager.GetOrCreatePageForWorkflowAsync(workflowInstanceId, context.CancellationToken);
                
                // First ensure page is loaded
                await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                
                // Determine the state to wait for
                var selectorState = State.ToLowerInvariant() switch
                {
                    "hidden" => WaitForSelectorState.Hidden,
                    "attached" => WaitForSelectorState.Attached,
                    "detached" => WaitForSelectorState.Detached,
                    _ => WaitForSelectorState.Visible
                };

                _logger.LogDebug("Waiting for selector '{Selector}' to be in state '{State}' (timeout: {Timeout}ms)...", 
                    selector, selectorState, timeout);
                
                // Wait for the selector with full timeout
                await page.WaitForSelectorAsync(selector, new() { 
                    State = selectorState,
                    Timeout = timeout
                });
                
                _logger.LogInformation("Successfully waited for selector {Selector} in state {State}", 
                    selector, selectorState);
                
                // For debugging - check what selectors are available on the page
                var selectorCount = await page.EvaluateAsync<int>($"document.querySelectorAll('{selector}').length");
                _logger.LogDebug("Found {Count} elements matching selector '{Selector}'", selectorCount, selector);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to wait for selector {Selector}: {Message}", selector, ex.Message);
                
                // Try to log page details to help with debugging
                try
                {
                    var page = await _browserSessionManager.GetOrCreatePageForWorkflowAsync(workflowInstanceId, context.CancellationToken);
                    var title = await page.TitleAsync();
                    var url = page.Url;
                    _logger.LogDebug("Current page info - Title: '{Title}', URL: '{Url}'", title, url);
                }
                catch (Exception debugEx)
                {
                    _logger.LogWarning("Could not get page debug info: {Message}", debugEx.Message);
                }
                
                throw new Exception($"Wait for selector operation failed: {ex.Message}", ex);
            }
        }
    }
} 