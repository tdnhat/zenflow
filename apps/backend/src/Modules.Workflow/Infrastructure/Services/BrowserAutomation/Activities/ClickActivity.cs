using Elsa.Extensions;
using Elsa.Workflows;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Modules.Workflow.DDD.Interfaces;

namespace Modules.Workflow.Infrastructure.Services.BrowserAutomation.Activities
{
    public class ClickActivity : CodeActivity
    {
        private readonly IBrowserSessionManager _browserSessionManager;
        private readonly ILogger<ClickActivity> _logger;

        public ClickActivity(
            IBrowserSessionManager browserSessionManager,
            ILogger<ClickActivity> logger)
        {
            _browserSessionManager = browserSessionManager;
            _logger = logger;
        }

        public string Selector { get; set; } = default!;
        public bool RequireVisible { get; set; } = true;
        public int Delay { get; set; } = 0;
        public bool Force { get; set; } = false;
        public int AfterDelay { get; set; } = 0;

        protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var workflowInstanceId = context.WorkflowExecutionContext.Id.ToString();
            _logger.LogInformation("Executing ClickActivity for workflow {WorkflowInstanceId} with selector {Selector}",
                workflowInstanceId, Selector);

            var page = await _browserSessionManager.GetOrCreatePageForWorkflowAsync(workflowInstanceId, context.CancellationToken);
            var selector = Selector;

            try
            {
                // Find the element
                _logger.LogDebug("Finding element with selector {Selector}", selector);
                var element = await page.QuerySelectorAsync(selector);
                if (element == null)
                {
                    _logger.LogError("Element not found with selector: {Selector}", selector);
                    throw new Exception($"Element not found with selector: {selector}");
                }

                // Check visibility if required
                if (RequireVisible)
                {
                    _logger.LogDebug("Checking visibility for element with selector {Selector}", selector);
                    var isVisible = await element.IsVisibleAsync();
                    if (!isVisible)
                    {
                        _logger.LogError("Element not visible: {Selector}", selector);
                        throw new Exception($"Element not visible: {selector}");
                    }
                }

                // Optional delay before clicking
                if (Delay > 0)
                {
                    _logger.LogDebug("Delaying {Delay}ms before clicking", Delay);
                    await Task.Delay(Delay, context.CancellationToken);
                }

                // Perform the click
                _logger.LogDebug("Clicking element with selector {Selector} (Force: {Force})", selector, Force);
                await element.ClickAsync(new() { Force = Force });
                _logger.LogInformation("Successfully clicked element with selector {Selector}", selector);

                // Optional delay after clicking
                if (AfterDelay > 0)
                {
                    _logger.LogDebug("Delaying {AfterDelay}ms after clicking", AfterDelay);
                    await Task.Delay(AfterDelay, context.CancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to click element with selector {Selector}: {ErrorMessage}", selector, ex.Message);
                throw new Exception($"Click operation failed: {ex.Message}", ex);
            }
        }
    }
}