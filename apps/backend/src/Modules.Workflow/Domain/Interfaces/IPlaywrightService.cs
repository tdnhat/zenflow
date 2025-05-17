namespace Modules.Workflow.Domain.Interfaces.Core
{
    public interface IPlaywrightService
    {
        Task<string> GetElementAttributeAsync(string url, string selector, string attributeName);
        Task<string> ExtractTextFromElementAsync(string url, string selector);
    }
}