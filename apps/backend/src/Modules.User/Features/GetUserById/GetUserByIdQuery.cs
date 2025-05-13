using MediatR;

namespace Modules.User.Features.GetUserById
{
    public record GetUserByIdQuery(Guid Id) : IRequest<GetUserByIdResponse?>;
}
