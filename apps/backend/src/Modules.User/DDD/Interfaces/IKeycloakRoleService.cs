using Modules.User.Dtos.Keycloak;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Modules.User.DDD.Interfaces
{
    /// <summary>
    /// Service for managing Keycloak roles
    /// </summary>
    public interface IKeycloakRoleService
    {
        /// Gets all available realm roles from Keycloak
        Task<List<RoleRepresentation>> GetAllRealmRolesAsync();

        /// Gets roles assigned to a user
        Task<List<RoleRepresentation>> GetUserRealmRoleMappingsAsync(string userId);

        /// Gets role names assigned to a user
        Task<List<string>> GetUserRoleNamesAsync(string userId);
    }
}