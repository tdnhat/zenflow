namespace Modules.User.DDD.Interfaces
{
    public interface IKeycloakTokenService
    {
        Task<string> GetAdminTokenAsync();
    }
}
