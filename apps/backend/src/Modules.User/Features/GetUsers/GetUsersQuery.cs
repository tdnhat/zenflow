using MediatR;
using Modules.User.Dtos;

namespace Modules.User.Features.GetUsers
{
    public record GetUsersQuery() : IRequest<List<UserDto>>;
}
