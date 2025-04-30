using MediatR;
using Modules.User.Dtos;

namespace Modules.User.Features.UpdateUserById
{
    public record UpdateUserByIdCommand(
        Guid Id,
        string? Username = null,
        List<string>? Roles = null,
        bool? IsActive = null
    ) : IRequest<UserDto?>;
}
