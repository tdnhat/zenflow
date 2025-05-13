using MediatR;

namespace Modules.User.Features.GetUsers
{
    public record GetUsersQuery() : IRequest<List<GetUsersResponse>>;
}
