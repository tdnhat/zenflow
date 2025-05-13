using Elsa.Extensions;
using Elsa.Workflows;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Modules.Workflow.DDD.Interfaces;

namespace Modules.Workflow.Infrastructure.Services.BrowserAutomation.Activities
{
    public class ScreenshotActivity : CodeActivity
    {
        private readonly IBrowserSessionManager _browserSessionManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ScreenshotActivity> _logger;

        public ScreenshotActivity(
            IBrowserSessionManager browserSessionManager,
            IWebHostEnvironment environment,
            ILogger<ScreenshotActivity> logger)
        {
            _browserSessionManager = browserSessionManager;
            _environment = environment;
            _logger = logger;
        }

        public bool FullPage { get; set; } = false;
        public string? Selector { get; set; }
        public string ScreenshotVariableName { get; set; } = "LastScreenshotPath";

        protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var workflowInstanceId = context.WorkflowExecutionContext.Id.ToString();
            _logger.LogInformation("Executing ScreenshotActivity for workflow {WorkflowInstanceId}", workflowInstanceId);

            var page = await _browserSessionManager.GetOrCreatePageForWorkflowAsync(workflowInstanceId, context.CancellationToken);

            try
            {
                byte[] screenshot;
                string screenshotType;

                // Take screenshot of specific element if selector is provided
                if (!string.IsNullOrEmpty(Selector))
                {
                    _logger.LogDebug("Taking screenshot of element with selector {Selector}", Selector);
                    var element = await page.QuerySelectorAsync(Selector);

                    if (element == null)
                    {
                        _logger.LogError("Element not found with selector: {Selector}", Selector);
                        throw new Exception($"Element not found with selector: {Selector}");
                    }

                    screenshot = await element.ScreenshotAsync();
                    screenshotType = "element";
                }
                else
                {
                    // Take full page or viewport screenshot
                    _logger.LogDebug("Taking {Type} screenshot", FullPage ? "full page" : "viewport");
                    var options = new PageScreenshotOptions
                    {
                        FullPage = FullPage,
                        Type = ScreenshotType.Png
                    };

                    screenshot = await page.ScreenshotAsync(options);
                    screenshotType = FullPage ? "fullpage" : "viewport";
                }

                _logger.LogDebug("Screenshot captured successfully, size: {Size} bytes", screenshot.Length);

                // Save the screenshot and get the path
                var screenshotPath = SaveScreenshot(screenshot, workflowInstanceId, screenshotType);
                _logger.LogInformation("Screenshot saved to {Path}", screenshotPath);

                // Store the screenshot path in a variable
                context.SetVariable(ScreenshotVariableName, screenshotPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to take screenshot: {ErrorMessage}", ex.Message);
                throw new Exception($"Screenshot operation failed: {ex.Message}", ex);
            }
        }

        private string SaveScreenshot(byte[] screenshot, string workflowInstanceId, string type)
        {
            // Create directory for screenshots if it doesn't exist
            var screenshotDir = Path.Combine(_environment.WebRootPath, "screenshots", workflowInstanceId);
            Directory.CreateDirectory(screenshotDir);

            // Generate unique filename
            var filename = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{type}.png";
            var path = Path.Combine(screenshotDir, filename);

            _logger.LogDebug("Saving screenshot to {AbsolutePath}", path);

            // Save the screenshot
            File.WriteAllBytes(path, screenshot);

            // Return the relative path for the client
            return $"/screenshots/{workflowInstanceId}/{filename}";
        }
    }
}