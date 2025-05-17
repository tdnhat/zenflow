using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Modules.Workflow.Domain.Interfaces.Core;
using System.Text;
using System.Text.Json;

namespace Modules.Workflow.Infrastructure.Services.AI
{
    public class AIService : IAIService
    {
        private readonly ILogger<AIService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _apiEndpoint;

        public AIService(
            ILogger<AIService> logger,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("AIService");
            _apiKey = configuration["AI:ApiKey"];
            _apiEndpoint = configuration["AI:SummarizationEndpoint"];

            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_apiEndpoint))
            {
                throw new InvalidOperationException("AI service configuration is missing. Please check your configuration.");
            }
        }

        public async Task<string> SummarizeTextAsync(string text, int? maxLength = null)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                _logger.LogWarning("Empty text provided for summarization");
                return string.Empty;
            }

            try
            {
                _logger.LogInformation("Summarizing text of length {TextLength}", text.Length);

                var request = new
                {
                    text = text,
                    max_length = maxLength
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json");

                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

                var response = await _httpClient.PostAsync(_apiEndpoint, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<SummarizationResponse>(responseContent);

                _logger.LogInformation("Successfully summarized text. Summary length: {SummaryLength}",
                    result?.Summary?.Length ?? 0);

                return result?.Summary ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error summarizing text");
                throw;
            }
        }

        private class SummarizationResponse
        {
            public string Summary { get; set; }
        }
    }
}