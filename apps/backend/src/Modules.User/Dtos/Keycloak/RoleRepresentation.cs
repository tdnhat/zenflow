using System.Text.Json.Serialization;

namespace Modules.User.Dtos.Keycloak
{
    public class RoleRepresentation
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }
}