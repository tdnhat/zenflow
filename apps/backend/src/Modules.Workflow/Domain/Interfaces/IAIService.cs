namespace Modules.Workflow.Domain.Interfaces.Core
{
    public interface IAIService
    {
        Task<string> SummarizeTextAsync(string text, int? maxLength = null);
    }
} 