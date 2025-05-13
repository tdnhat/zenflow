using System.Collections.Generic;

namespace Modules.User.Infrastructure.Services.Keycloak.KeycloakDtos
{
    public record RoleRepresentation(
        string Id,
        string Name,
        string? Description = null,
        Dictionary<string, List<string>>? Attributes = null
    );
}