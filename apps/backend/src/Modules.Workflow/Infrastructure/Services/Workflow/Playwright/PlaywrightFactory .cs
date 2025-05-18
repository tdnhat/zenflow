using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Modules.Workflow.Domain.Interfaces.Core;

namespace Modules.Workflow.Infrastructure.Services.Workflow.Playwright
{
    public class PlaywrightFactory : IPlaywrightFactory
    {
        private readonly ILogger<PlaywrightFactory> _logger;

        public PlaywrightFactory(ILogger<PlaywrightFactory> logger)
        {
            _logger = logger;
        }

        public async Task<IPlaywright> CreateAsync()
        {
            try
            {
                // Ensure browsers are installed
                // This is typically done during application startup or deployment
                // Microsoft.Playwright.Program.Main(new[] { "install", "chromium" });
                
                return await Microsoft.Playwright.Playwright.CreateAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create Playwright instance");
                throw;
            }
        }
    }
}