using System.Collections.Generic;

namespace Modules.User.Infrastructure.Services.Keycloak.KeycloakDtos
{
    public record KeycloakUser(
        string Id,
        string Username,
        string Email,
        bool Enabled,
        List<string> RealmRoles
    );
}