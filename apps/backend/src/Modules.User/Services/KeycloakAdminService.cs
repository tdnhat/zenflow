using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Modules.User.DDD.Interfaces;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Modules.User.Repositories
{
    public class KeycloakAdminService : IKeycloakAdminService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<KeycloakAdminService> _logger;
        private readonly KeycloakSettings _settings;
        private readonly JsonSerializerOptions _jsonOptions;

        // Cache the admin token to avoid frequent token requests
        private string? _cachedAdminToken;
        private DateTime _tokenExpiryTime = DateTime.MinValue;

        public KeycloakAdminService(
            HttpClient httpClient,
            ILogger<KeycloakAdminService> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;

            // Extract settings from configuration
            _settings = new KeycloakSettings(
                BaseUrl: configuration["Keycloak:BaseUrl"]
                    ?? throw new ArgumentNullException("Keycloak:BaseUrl configuration is missing"),
                Realm: configuration["Keycloak:Realm"]
                    ?? throw new ArgumentNullException("Keycloak:Realm configuration is missing"),
                AdminClientId: configuration["Keycloak:AdminClientId"]
                    ?? throw new ArgumentNullException("Keycloak:AdminClientId configuration is missing"),
                AdminClientSecret: configuration["Keycloak:AdminClientSecret"]
                    ?? throw new ArgumentNullException("Keycloak:AdminClientSecret configuration is missing")
            );

            // Configure JSON serialization options
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        public async Task<KeycloakUser?> GetUserByIdAsync(string keycloakId)
        {
            _logger.LogDebug("Getting user with ID {UserId} from Keycloak", keycloakId);

            try
            {
                var token = await GetAdminTokenAsync();
                var requestUrl = $"{_settings.AdminApiUrl}/users/{keycloakId}";

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.GetAsync(requestUrl);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        _logger.LogWarning("User {UserId} not found in Keycloak", keycloakId);
                        return null;
                    }

                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to get Keycloak user {UserId}. Status: {Status}, Error: {Error}",
                        keycloakId, response.StatusCode, errorContent);
                    throw new KeycloakApiException($"Failed to get user {keycloakId}", response.StatusCode, errorContent);
                }

                var userJson = await response.Content.ReadFromJsonAsync<JsonElement>();
                var realmRoles = await GetUserRealmRolesAsync(keycloakId, token);

                return new KeycloakUser(
                    Id: userJson.GetProperty("id").GetString() ?? string.Empty,
                    Username: userJson.GetProperty("username").GetString() ?? string.Empty,
                    Email: userJson.TryGetProperty("email", out var emailProp) ? emailProp.GetString() ?? string.Empty : string.Empty,
                    Enabled: userJson.GetProperty("enabled").GetBoolean(),
                    RealmRoles: realmRoles
                );
            }
            catch (KeycloakApiException)
            {
                throw; // Already logged
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserId} from Keycloak", keycloakId);
                throw new KeycloakApiException($"Failed to get user {keycloakId}", ex);
            }
        }

        public async Task SetUserActiveStatusAsync(string keycloakId, bool isActive)
        {
            _logger.LogDebug("Setting user {UserId} active status to {IsActive}", keycloakId, isActive);

            try
            {
                var token = await GetAdminTokenAsync();
                var requestUrl = $"{_settings.AdminApiUrl}/users/{keycloakId}";

                var updateData = new Dictionary<string, object>
                {
                    ["enabled"] = isActive
                };

                await SendApiRequestAsync(HttpMethod.Put, requestUrl, token, updateData);
                _logger.LogInformation("Successfully updated user {UserId} active status to {IsActive}", keycloakId, isActive);
            }
            catch (Exception ex) when (ex is not KeycloakApiException)
            {
                _logger.LogError(ex, "Error setting user {UserId} active status", keycloakId);
                throw new KeycloakApiException($"Failed to update user {keycloakId} active status", ex);
            }
        }

        public async Task UpdateUserRolesAsync(string keycloakId, List<string> roles)
        {
            if (roles == null || roles.Count == 0)
            {
                _logger.LogWarning("No roles provided for user {UserId}, skipping role update", keycloakId);
                return;
            }

            _logger.LogDebug("Updating roles for user {UserId} to: {Roles}", keycloakId, string.Join(", ", roles));

            try
            {
                var token = await GetAdminTokenAsync();

                // Get all available roles first
                var availableRoles = await GetAllRealmRolesAsync(token);
                var rolesToAssign = new List<RoleRepresentation>();

                foreach (var roleName in roles)
                {
                    var role = availableRoles.Find(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
                    if (role != null)
                    {
                        rolesToAssign.Add(role);
                    }
                    else
                    {
                        _logger.LogWarning("Role {RoleName} not found in Keycloak, skipping", roleName);
                    }
                }

                if (rolesToAssign.Count == 0)
                {
                    _logger.LogWarning("None of the requested roles exist in Keycloak, nothing to assign");
                    return;
                }

                // First, remove all existing roles
                var currentRoles = await GetUserRealmRoleMappingsAsync(keycloakId, token);
                if (currentRoles.Count > 0)
                {
                    var removeUrl = $"{_settings.AdminApiUrl}/users/{keycloakId}/role-mappings/realm";
                    await SendApiRequestAsync(HttpMethod.Delete, removeUrl, token, currentRoles);
                }

                // Then assign the new roles
                var assignUrl = $"{_settings.AdminApiUrl}/users/{keycloakId}/role-mappings/realm";
                await SendApiRequestAsync(HttpMethod.Post, assignUrl, token, rolesToAssign);

                _logger.LogInformation("Successfully updated roles for user {UserId}", keycloakId);
            }
            catch (Exception ex) when (ex is not KeycloakApiException)
            {
                _logger.LogError(ex, "Error updating roles for user {UserId}", keycloakId);
                throw new KeycloakApiException($"Failed to update roles for user {keycloakId}", ex);
            }
        }

        public async Task UpdateUserUsernameAsync(string keycloakId, string newUsername)
        {
            if (string.IsNullOrWhiteSpace(newUsername))
            {
                throw new ArgumentException("Username cannot be empty", nameof(newUsername));
            }

            _logger.LogDebug("Updating username for user {UserId} to {NewUsername}", keycloakId, newUsername);

            try
            {
                // First check if the user is from a federated provider by getting their details
                var token = await GetAdminTokenAsync();
                var userCheckUrl = $"{_settings.AdminApiUrl}/users/{keycloakId}";

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var userResponse = await _httpClient.GetAsync(userCheckUrl);

                if (!userResponse.IsSuccessStatusCode)
                {
                    var errorContent = await userResponse.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to get user {UserId} details. Status: {Status}, Error: {Error}",
                        keycloakId, userResponse.StatusCode, errorContent);
                    throw new KeycloakApiException($"Failed to get user {keycloakId} details", userResponse.StatusCode, errorContent);
                }

                var userJson = await userResponse.Content.ReadFromJsonAsync<JsonElement>();

                // Check if user is federated/linked to external provider
                if (userJson.TryGetProperty("federationLink", out _) ||
                    userJson.TryGetProperty("federatedIdentities", out var fedIdentities) &&
                    fedIdentities.GetArrayLength() > 0)
                {
                    // User is federated, username likely can't be modified
                    _logger.LogWarning(
                        "User {UserId} is managed by an external identity provider. Username cannot be modified in Keycloak.",
                        keycloakId);

                    // We'll return successfully but log that we skipped the update
                    return;
                }

                // If user is not federated, proceed with username update
                var requestUrl = $"{_settings.AdminApiUrl}/users/{keycloakId}";

                var updateData = new Dictionary<string, object>
                {
                    ["username"] = newUsername
                };

                await SendApiRequestAsync(HttpMethod.Put, requestUrl, token, updateData);
                _logger.LogInformation("Successfully updated username for user {UserId} to {NewUsername}", keycloakId, newUsername);
            }
            catch (Exception ex) when (ex is not KeycloakApiException)
            {
                _logger.LogError(ex, "Error updating username for user {UserId}", keycloakId);
                throw new KeycloakApiException($"Failed to update username for user {keycloakId}", ex);
            }
        }

        #region Private helpers

        private async Task<string> GetAdminTokenAsync()
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

                var requestData = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "client_credentials",
                    ["client_id"] = _settings.AdminClientId,
                    ["client_secret"] = _settings.AdminClientSecret
                });

                var response = await _httpClient.PostAsync(tokenUrl, requestData);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to get admin token. Status: {Status}, Error: {Error}",
                        response.StatusCode, errorContent);
                    throw new KeycloakApiException("Failed to get admin token", response.StatusCode, errorContent);
                }

                var tokenResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
                var accessToken = tokenResponse.GetProperty("access_token").GetString();
                var expiresIn = tokenResponse.GetProperty("expires_in").GetInt32();

                // Cache the token
                _cachedAdminToken = accessToken;
                _tokenExpiryTime = DateTime.UtcNow.AddSeconds(expiresIn);

                _logger.LogDebug("Successfully obtained new admin token, expires in {ExpiresIn} seconds", expiresIn);

                return accessToken!;
            }
            catch (Exception ex) when (ex is not KeycloakApiException)
            {
                _logger.LogError(ex, "Error getting admin token from Keycloak");
                throw new KeycloakApiException("Failed to get admin token", ex);
            }
        }

        private async Task<List<string>> GetUserRealmRolesAsync(string userId, string token)
        {
            try
            {
                var mappings = await GetUserRealmRoleMappingsAsync(userId, token);
                return mappings.Select(r => r.Name).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting realm roles for user {UserId}", userId);
                return new List<string>();
            }
        }

        private async Task<List<RoleRepresentation>> GetUserRealmRoleMappingsAsync(string userId, string token)
        {
            var url = $"{_settings.AdminApiUrl}/users/{userId}/role-mappings/realm";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get user role mappings, status: {Status}", response.StatusCode);
                return new List<RoleRepresentation>();
            }

            return await response.Content.ReadFromJsonAsync<List<RoleRepresentation>>(_jsonOptions) ?? new List<RoleRepresentation>();
        }

        private async Task<List<RoleRepresentation>> GetAllRealmRolesAsync(string token)
        {
            var url = $"{_settings.AdminApiUrl}/roles";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to get realm roles. Status: {Status}, Error: {Error}",
                    response.StatusCode, errorContent);
                throw new KeycloakApiException("Failed to get realm roles", response.StatusCode, errorContent);
            }

            return await response.Content.ReadFromJsonAsync<List<RoleRepresentation>>(_jsonOptions) ?? new List<RoleRepresentation>();
        }

        private async Task SendApiRequestAsync<T>(HttpMethod method, string url, string token, T data)
        {
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

        #endregion

        #region Models

        private record KeycloakSettings(
            string BaseUrl,
            string Realm,
            string AdminClientId,
            string AdminClientSecret
        )
        {
            public string AdminApiUrl => $"{BaseUrl.TrimEnd('/')}/admin/realms/{Realm}";
        }

        private class RoleRepresentation
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;

            [JsonPropertyName("name")]
            public string Name { get; set; } = string.Empty;
        }

        #endregion
    }

    public class KeycloakApiException : Exception
    {
        public System.Net.HttpStatusCode? StatusCode { get; }
        public string? ResponseContent { get; }

        public KeycloakApiException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public KeycloakApiException(string message, System.Net.HttpStatusCode statusCode, string responseContent)
            : base($"{message}. Status: {statusCode}, Response: {responseContent}")
        {
            StatusCode = statusCode;
            ResponseContent = responseContent;
        }
    }
}