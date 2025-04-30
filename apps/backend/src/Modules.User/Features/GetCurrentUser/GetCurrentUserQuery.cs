using MediatR;
using Modules.User.Dtos;

namespace Modules.User.Features.GetCurrentUser
{
    public record GetCurrentUserQuery(string ExternalId, string Username, string Email) : IRequest<UserDto>;
}
