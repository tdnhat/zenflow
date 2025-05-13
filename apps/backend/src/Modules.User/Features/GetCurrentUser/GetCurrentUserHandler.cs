using MediatR;
using Modules.User.Domain.Interfaces;
using System.Threading.Tasks;

namespace Modules.User.Features.GetCurrentUser
{
    internal class GetCurrentUserHandler : IRequestHandler<GetCurrentUserQuery, GetCurrentUserResponse>
    {
        private readonly IUserRepository _userRepo;
        public GetCurrentUserHandler(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }
        public async Task<GetCurrentUserResponse> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepo.GetByExternalIdAsync(request.ExternalId, cancellationToken);

            if (user is null)
            {
                user = Domain.Entities.User.Create(request.ExternalId, request.Username, request.Email);
                await _userRepo.AddAsync(user, cancellationToken);
            }

            return new GetCurrentUserResponse(user.Id, user.Username, user.Email);
        }
    }
}
