using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Modules.User.DDD.Interfaces;
using System.Text.Json;
using ZenFlow.Shared.Configurations;

namespace Modules.User.Services
{
    public class KeycloakTokenService : IKeycloakTokenService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<KeycloakTokenService> _logger;
        private readonly KeycloakSettings _settings;
        private string? _cachedAdminToken;
        private DateTime _tokenExpiryTime = DateTime.MinValue;

        public KeycloakTokenService(HttpClient httpClient, ILogger<KeycloakTokenService> logger, IConfiguration config)
        {
            _httpClient = httpClient;
            _logger = logger;
            _settings = config.GetSection("Keycloak").Get<KeycloakSettings>() ?? throw new ArgumentException("Keycloak settings missing");
        }

        public async Task<string> GetAdminTokenAsync()
        {
            if (_cachedAdminToken != null && DateTime.UtcNow.AddSeconds(30) < _tokenExpiryTime)
            {
                return _cachedAdminToken;
            }

            _logger.LogDebug("Requesting new admin token from Keycloak");
            var url = $"{_settings.BaseUrl}/realms/{_settings.Realm}/protocol/openid-connect/token";

            var form = new Dictionary<string, string>
        {
            { "client_id", _settings.AdminClientId },
            { "client_secret", _settings.AdminClientSecret },
            { "grant_type", "client_credentials" }
        };

            var response = await _httpClient.PostAsync(url, new FormUrlEncodedContent(form));
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var accessToken = doc.RootElement.GetProperty("access_token").GetString();
            var expiresIn = doc.RootElement.GetProperty("expires_in").GetInt32();

            _cachedAdminToken = accessToken;
            _tokenExpiryTime = DateTime.UtcNow.AddSeconds(expiresIn);
            return accessToken!;
        }
    }


}
