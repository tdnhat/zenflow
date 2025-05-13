using MediatR;
using Modules.User.Features.CreateUser;

namespace Modules.User.Features.UpdateUserById
{
    public record UpdateUserByIdCommand(
        Guid Id,
        string? Username = null,
        List<string>? Roles = null,
        bool? IsActive = null
    ) : IRequest<UpdateUserResponse?>;
}
