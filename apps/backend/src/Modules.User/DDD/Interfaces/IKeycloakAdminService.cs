using System.Threading.Tasks;
using System.Collections.Generic;

namespace Modules.User.DDD.Interfaces
{
    public interface IKeycloakAdminService
    {
        Task UpdateUserUsernameAsync(string keycloakId, string newUsername);

        Task UpdateUserRolesAsync(string keycloakId, List<string> roles);

        Task SetUserActiveStatusAsync(string keycloakId, bool isActive);

        Task<KeycloakUser?> GetUserByIdAsync(string keycloakId);
    }

    public record KeycloakUser(
        string Id,
        string Username,
        string Email,
        bool Enabled,
        List<string> RealmRoles
    );
}