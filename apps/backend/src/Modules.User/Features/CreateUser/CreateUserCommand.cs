using MediatR;
using Modules.User.Dtos;

namespace Modules.User.Features.CreateUser
{
    public record CreateUserCommand(string ExternalId, string Username, string Email) : IRequest<UserDto>;
}
