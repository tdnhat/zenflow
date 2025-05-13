using MediatR;

namespace Modules.User.Features.CreateUser
{
    public record CreateUserCommand(string ExternalId, string Username, string Email) : IRequest<CreateUserResponse>;
}
