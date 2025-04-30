using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Modules.User.DDD.Interfaces;
using System.Text.Json;
using ZenFlow.Shared.Configurations;

namespace Modules.User.Services
{
    /// Service for obtaining and caching Keycloak admin tokens
    public class KeycloakTokenService : IKeycloakTokenService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<KeycloakTokenService> _logger;
        private readonly KeycloakSettings _settings;
        
        // Cache the admin token to avoid frequent token requests
        private string? _cachedAdminToken;
        private DateTime _tokenExpiryTime = DateTime.MinValue;

        public KeycloakTokenService(
            HttpClient httpClient, 
            ILogger<KeycloakTokenService> logger, 
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            
            // Extract settings from configuration
            _settings = configuration.GetSection("Keycloak").Get<KeycloakSettings>() 
                ?? throw new ArgumentException("Keycloak settings missing");
        }

        /// Gets a valid admin token, either from cache or by requesting a new one
        public async Task<string> GetAdminTokenAsync()
        {
            // Return cached token if it's still valid (with 30 second buffer)
            if (_cachedAdminToken != null && DateTime.UtcNow.AddSeconds(30) < _tokenExpiryTime)
            {
                return _cachedAdminToken;
            }

            _logger.LogDebug("Requesting new admin token from Keycloak");

            try
            {
                var tokenUrl = $"{_settings.BaseUrl}/realms/{_settings.Realm}/protocol/openid-connect/token";

                var form = new Dictionary<string, string>
                {
                    ["client_id"] = _settings.AdminClientId,
                    ["client_secret"] = _settings.AdminClientSecret,
                    ["grant_type"] = "client_credentials"
                };

                var response = await _httpClient.PostAsync(tokenUrl, new FormUrlEncodedContent(form));
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);
                var accessToken = doc.RootElement.GetProperty("access_token").GetString();
                var expiresIn = doc.RootElement.GetProperty("expires_in").GetInt32();

                // Cache the token
                _cachedAdminToken = accessToken;
                _tokenExpiryTime = DateTime.UtcNow.AddSeconds(expiresIn);

                _logger.LogDebug("Successfully obtained new admin token, expires in {ExpiresIn} seconds", expiresIn);

                return accessToken!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin token from Keycloak");
                throw;
            }
        }
    }
}
