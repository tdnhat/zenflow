using System.ComponentModel.DataAnnotations;

namespace ZenFlow.Shared.Configurations
{
    public class KeycloakSettings
    {
        [Required(ErrorMessage = "BaseUrl is required.")]
        public string BaseUrl { get; set; } = default!;
        [Required(ErrorMessage = "Realm is required.")]
        public string Realm { get; set; } = default!;
        [Required(ErrorMessage = "ClientId is required.")]
        public string AdminClientId { get; set; } = default!;
        [Required(ErrorMessage = "ClientSecret is required.")]
        public string AdminClientSecret { get; set; } = default!;

        // Computed property for the admin API URL
        public string AdminApiUrl => $"{BaseUrl.TrimEnd('/')}/admin/realms/{Realm}";
    }
}
