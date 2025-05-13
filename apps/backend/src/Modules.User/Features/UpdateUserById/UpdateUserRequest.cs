namespace Modules.User.Features.UpdateUserById
{
    public record UpdateUserRequest(
        string? Username = null,
        List<string>? Roles = null,
        bool? IsActive = null
    );
} 