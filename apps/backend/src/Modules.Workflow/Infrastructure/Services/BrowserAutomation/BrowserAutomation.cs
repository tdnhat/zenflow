using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Modules.Workflow.DDD.Interfaces;

namespace Modules.Workflow.Infrastructure.Services.BrowserAutomation
{
    public class BrowserAutomation : IBrowserAutomation
    {
        private readonly ILogger<BrowserAutomation> _logger;
        public BrowserAutomation(ILogger<BrowserAutomation> logger)
        {
            _logger = logger;
        }

        public async Task<IBrowser> LaunchBrowserAsync(BrowserLaunchOptions options)
        {
            try
            {
                // Install Playwright browsers
                var exitCode = Program.Main(new[] { "install", "chromium" });
                if (exitCode != 0)
                {
                    _logger.LogWarning("Failed to install Playwright browsers");
                }

                // Create a new Playwright instance
                var playwright = await Playwright.CreateAsync();

                // Launch the browser
                return await playwright.Chromium.LaunchAsync(new()
                {
                    Headless = options.Headless,
                    Args = options.Args
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to launch browser");
                throw;
            }
        }
    }
}
