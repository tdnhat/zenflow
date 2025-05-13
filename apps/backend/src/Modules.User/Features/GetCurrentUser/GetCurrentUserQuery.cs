using MediatR;

namespace Modules.User.Features.GetCurrentUser
{
    public record GetCurrentUserQuery(string ExternalId, string Username, string Email) : IRequest<GetCurrentUserResponse>;
}
