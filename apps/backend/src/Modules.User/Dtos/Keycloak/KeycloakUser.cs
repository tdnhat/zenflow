using System.Collections.Generic;

namespace Modules.User.Dtos.Keycloak
{
    public record KeycloakUser(
        string Id,
        string Username,
        string Email,
        bool Enabled,
        List<string> RealmRoles
    );
}
