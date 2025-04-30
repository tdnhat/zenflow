namespace Modules.User.Dtos
{
    public record UpdateUserRequest(
        string? Username = null,
        List<string>? Roles = null,
        bool? IsActive = null
    );
}
