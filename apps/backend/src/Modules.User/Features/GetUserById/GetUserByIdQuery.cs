using MediatR;
using Modules.User.Dtos;

namespace Modules.User.Features.GetUserById
{
    public record GetUserByIdQuery(Guid Id) : IRequest<UserDto?>;
}
