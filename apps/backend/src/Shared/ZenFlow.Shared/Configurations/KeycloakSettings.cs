namespace ZenFlow.Shared.Configurations
{
    public class KeycloakSettings
    {
        public string BaseUrl { get; set; } = default!;
        public string Realm { get; set; } = default!;
        public string AdminClientId { get; set; } = default!;
        public string AdminClientSecret { get; set; } = default!;
    }
}
