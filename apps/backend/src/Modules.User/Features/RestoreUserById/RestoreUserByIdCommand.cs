using MediatR;

namespace Modules.User.Features.RestoreUserById
{
    public record RestoreUserByIdCommand(Guid Id) : IRequest;
}
