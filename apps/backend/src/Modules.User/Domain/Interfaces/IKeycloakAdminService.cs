using Modules.User.Infrastructure.Services.Keycloak.KeycloakDtos;

namespace Modules.User.Domain.Interfaces
{
    /// Service for managing users in Keycloak
    public interface IKeycloakAdminService
    {
        Task UpdateUserUsernameAsync(string keycloakId, string newUsername);

        Task UpdateUserRolesAsync(string keycloakId, List<string> roles);

        Task SetUserActiveStatusAsync(string keycloakId, bool isActive);

        Task<KeycloakUser?> GetUserByIdAsync(string keycloakId);
        Task DeleteUserAsync(string keycloakId);
    }
}