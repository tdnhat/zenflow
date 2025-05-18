using Microsoft.Extensions.Logging;
using Modules.Workflow.Domain.Interfaces.Core;

namespace Modules.Workflow.Infrastructure.Services.AI
{
    /// <summary>
    /// A mock implementation of the AI service that can be used for development and testing.
    /// This avoids making actual API calls to OpenAI or other AI providers.
    /// </summary>
    public class MockAIService : IAIService
    {
        private readonly ILogger<MockAIService> _logger;

        public MockAIService(ILogger<MockAIService> logger)
        {
            _logger = logger;
        }

        public Task<string> SummarizeTextAsync(string text, int? maxLength = null)
        {
            _logger.LogInformation("Mock AI service summarizing text of length {TextLength}", text?.Length ?? 0);

            // For testing purposes, return a mock summary based on the original text
            if (string.IsNullOrWhiteSpace(text))
            {
                return Task.FromResult("No text provided for summarization.");
            }

            // Create a simplified "summary" by taking the first few sentences
            var sentences = text.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            
            // Take first 3 sentences or fewer if there aren't 3
            var summaryLength = Math.Min(3, sentences.Length);
            var summary = string.Join(". ", sentences.Take(summaryLength)).Trim() + ".";
            
            // Apply maxLength if specified
            if (maxLength.HasValue && summary.Length > maxLength.Value)
            {
                summary = summary.Substring(0, maxLength.Value - 3) + "...";
            }

            _logger.LogInformation("Mock summary created with length {SummaryLength}", summary.Length);
            
            return Task.FromResult(summary);
        }
    }
} 