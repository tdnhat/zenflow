using Microsoft.Extensions.Logging;
using Modules.User.DDD.Interfaces;
using Modules.User.Dtos.Keycloak;
using Modules.User.Infrastructure.Exceptions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using ZenFlow.Shared.Configurations;

namespace Modules.User.Services
{
    /// Service for managing Keycloak users through the Keycloak Admin API
    public class KeycloakAdminService : IKeycloakAdminService
    {
        private readonly ILogger<KeycloakAdminService> _logger;
        private readonly IKeycloakHttpClient _keycloakHttp;
        private readonly IKeycloakRoleService _roleService;
        private readonly KeycloakSettings _settings;

        public KeycloakAdminService(
            ILogger<KeycloakAdminService> logger,
            IKeycloakHttpClient keycloakHttp,
            IKeycloakRoleService roleService,
            KeycloakSettings settings)
        {
            _logger = logger;
            _keycloakHttp = keycloakHttp;
            _roleService = roleService;
            _settings = settings;
        }

        public async Task<KeycloakUser?> GetUserByIdAsync(string keycloakId)
        {
            _logger.LogDebug("Getting user with ID {UserId} from Keycloak", keycloakId);

            try
            {
                var requestUrl = $"{_settings.AdminApiUrl}/users/{keycloakId}";
                var response = await _keycloakHttp.SendApiRequestWithResponseAsync(HttpMethod.Get, requestUrl);

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
                var realmRoles = await _roleService.GetUserRoleNamesAsync(keycloakId);

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
                var requestUrl = $"{_settings.AdminApiUrl}/users/{keycloakId}";

                var updateData = new Dictionary<string, object>
                {
                    ["enabled"] = isActive
                };

                await _keycloakHttp.SendApiRequestAsync(HttpMethod.Put, requestUrl, updateData);
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
                // Get all available roles first
                var availableRoles = await _roleService.GetAllRealmRolesAsync();
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
                var currentRoles = await _roleService.GetUserRealmRoleMappingsAsync(keycloakId);
                if (currentRoles.Count > 0)
                {
                    var removeUrl = $"{_settings.AdminApiUrl}/users/{keycloakId}/role-mappings/realm";
                    await _keycloakHttp.SendApiRequestAsync(HttpMethod.Delete, removeUrl, currentRoles);
                }

                // Then assign the new roles
                var assignUrl = $"{_settings.AdminApiUrl}/users/{keycloakId}/role-mappings/realm";
                await _keycloakHttp.SendApiRequestAsync(HttpMethod.Post, assignUrl, rolesToAssign);

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
                var userCheckUrl = $"{_settings.AdminApiUrl}/users/{keycloakId}";
                var userResponse = await _keycloakHttp.SendApiRequestWithResponseAsync(HttpMethod.Get, userCheckUrl);

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

                await _keycloakHttp.SendApiRequestAsync(HttpMethod.Put, requestUrl, updateData);
                _logger.LogInformation("Successfully updated username for user {UserId} to {NewUsername}", keycloakId, newUsername);
            }
            catch (Exception ex) when (ex is not KeycloakApiException)
            {
                _logger.LogError(ex, "Error updating username for user {UserId}", keycloakId);
                throw new KeycloakApiException($"Failed to update username for user {keycloakId}", ex);
            }
        }

        public async Task DeleteUserAsync(string keycloakId)
        {
            _logger.LogDebug("Deleting user {UserId} from Keycloak", keycloakId);

            try
            {
                var requestUrl = $"{_settings.AdminApiUrl}/users/{keycloakId}";
                
                // Use SendApiRequestWithResponseAsync instead of SendApiRequestAsync for DELETE requests
                var response = await _keycloakHttp.SendApiRequestWithResponseAsync(HttpMethod.Delete, requestUrl);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to delete Keycloak user {UserId}. Status: {Status}, Error: {Error}",
                        keycloakId, response.StatusCode, errorContent);
                    throw new KeycloakApiException($"Failed to delete user {keycloakId}", response.StatusCode, errorContent);
                }
                
                _logger.LogInformation("Successfully deleted user {UserId} from Keycloak", keycloakId);
            }
            catch (KeycloakApiException)
            {
                throw; // Already logged
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId} from Keycloak", keycloakId);
                throw new KeycloakApiException($"Failed to delete user {keycloakId}", ex);
            }
        }
    }
}