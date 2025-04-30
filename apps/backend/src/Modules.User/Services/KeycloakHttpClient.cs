using Microsoft.Extensions.Logging;
using Modules.User.DDD.Interfaces;
using Modules.User.Infrastructure.Exceptions;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Modules.User.Services
{
    /// Helper class for making HTTP requests to the Keycloak API
    public class KeycloakHttpClient : IKeycloakHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<KeycloakHttpClient> _logger;
        private readonly IKeycloakTokenService _tokenService;
        private readonly JsonSerializerOptions _jsonOptions;

        public KeycloakHttpClient(
            HttpClient httpClient,
            ILogger<KeycloakHttpClient> logger,
            IKeycloakTokenService tokenService)
        {
            _httpClient = httpClient;
            _logger = logger;
            _tokenService = tokenService;
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
        }

        /// Sends an API request to Keycloak with proper authentication
        public async Task SendApiRequestAsync<T>(HttpMethod method, string url, T data)
        {
            try
            {
                var token = await _tokenService.GetAdminTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                using var request = new HttpRequestMessage(method, url);

                if (data != null && method != HttpMethod.Get)
                {
                    var json = JsonSerializer.Serialize(data, _jsonOptions);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Keycloak API request failed. URL: {Url}, Method: {Method}, Status: {Status}, Error: {Error}",
                        url, method, response.StatusCode, errorContent);
                    throw new KeycloakApiException($"Keycloak API request failed: {method} {url}", response.StatusCode, errorContent);
                }
            }
            catch (Exception ex) when (ex is not KeycloakApiException)
            {
                _logger.LogError(ex, "Error making request to Keycloak API");
                throw new KeycloakApiException($"Request failed: {method} {url}", ex);
            }
        }

        /// Sends an API request to Keycloak with proper authentication and returns the response
        public async Task<HttpResponseMessage> SendApiRequestWithResponseAsync(HttpMethod method, string url, object? data = null)
        {
            try
            {
                var token = await _tokenService.GetAdminTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                using var request = new HttpRequestMessage(method, url);

                if (data != null && method != HttpMethod.Get)
                {
                    var json = JsonSerializer.Serialize(data, _jsonOptions);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                return await _httpClient.SendAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error making request to Keycloak API");
                throw new KeycloakApiException($"Request failed: {method} {url}", ex);
            }
        }
        
        /// Gets the JSON serializer options used for Keycloak API requests
        public JsonSerializerOptions JsonOptions => _jsonOptions;
    }
}