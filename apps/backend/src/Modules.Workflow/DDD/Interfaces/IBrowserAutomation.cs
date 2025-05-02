using Microsoft.Playwright;

namespace Modules.Workflow.DDD.Interfaces
{
    public interface IBrowserAutomation
    {
        Task<IBrowser> LaunchBrowserAsync(BrowserLaunchOptions options);
    }

    public class BrowserLaunchOptions
    {
        public bool Headless { get; set; } = true;
        public string[]? Args { get; set; }
    }
}
