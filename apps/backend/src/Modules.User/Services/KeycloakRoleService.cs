using Microsoft.Extensions.Logging;
using Modules.User.DDD.Interfaces;
using Modules.User.Dtos.Keycloak;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ZenFlow.Shared.Configurations;

namespace Modules.User.Services
{
    /// Service for managing Keycloak roles
    public class KeycloakRoleService : IKeycloakRoleService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<KeycloakRoleService> _logger;
        private readonly IKeycloakTokenService _tokenService;
        private readonly KeycloakSettings _settings;
        private readonly JsonSerializerOptions _jsonOptions;

        public KeycloakRoleService(
            HttpClient httpClient,
            ILogger<KeycloakRoleService> logger,
            IKeycloakTokenService tokenService,
            KeycloakSettings settings)
        {
            _httpClient = httpClient;
            _logger = logger;
            _tokenService = tokenService;
            _settings = settings;
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
        }

        /// Gets all realm roles from Keycloak
        public async Task<List<RoleRepresentation>> GetAllRealmRolesAsync()
        {
            var token = await _tokenService.GetAdminTokenAsync();
            var url = $"{_settings.AdminApiUrl}/roles";
            
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<RoleRepresentation>>(_jsonOptions) 
                ?? new List<RoleRepresentation>();
        }

        /// Gets roles assigned to a user
        public async Task<List<RoleRepresentation>> GetUserRealmRoleMappingsAsync(string userId)
        {
            try
            {
                var token = await _tokenService.GetAdminTokenAsync();
                var url = $"{_settings.AdminApiUrl}/users/{userId}/role-mappings/realm";
                
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get user role mappings, status: {Status}", response.StatusCode);
                    return new List<RoleRepresentation>();
                }

                return await response.Content.ReadFromJsonAsync<List<RoleRepresentation>>(_jsonOptions) 
                    ?? new List<RoleRepresentation>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting realm roles for user {UserId}", userId);
                return new List<RoleRepresentation>();
            }
        }

        /// Gets role names assigned to a user
        public async Task<List<string>> GetUserRoleNamesAsync(string userId)
        {
            var roles = await GetUserRealmRoleMappingsAsync(userId);
            return roles.Select(r => r.Name).ToList();
        }
    }
}