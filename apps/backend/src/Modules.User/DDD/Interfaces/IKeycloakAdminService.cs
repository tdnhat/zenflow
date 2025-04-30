using System.Threading.Tasks;
using System.Collections.Generic;
using Modules.User.Dtos.Keycloak;

namespace Modules.User.DDD.Interfaces
{
    /// Service for managing users in Keycloak
    public interface IKeycloakAdminService
    {
        /// Updates a user's username in Keycloak
        Task UpdateUserUsernameAsync(string keycloakId, string newUsername);

        /// Updates a user's roles in Keycloak
        Task UpdateUserRolesAsync(string keycloakId, List<string> roles);

        /// Sets a user's active status in Keycloak
        Task SetUserActiveStatusAsync(string keycloakId, bool isActive);

        /// Retrieves user information from Keycloak by ID
        Task<KeycloakUser?> GetUserByIdAsync(string keycloakId);
    }
}