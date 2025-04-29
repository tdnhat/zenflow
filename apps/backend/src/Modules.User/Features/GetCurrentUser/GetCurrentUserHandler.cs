using MediatR;
using Modules.User.DDD.Interfaces;
using Modules.User.Dtos;

namespace Modules.User.Features.GetCurrentUser
{
    internal class GetCurrentUserHandler : IRequestHandler<GetCurrentUserQuery, UserDto>
    {
        private readonly IUserRepository _userRepo;
        public GetCurrentUserHandler(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }
        public async Task<UserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepo.GetByExternalIdAsync(request.ExternalId, cancellationToken);

            if (user is null)
            {
                user = DDD.Entities.User.Create(request.ExternalId, request.Username, request.Email);
                await _userRepo.AddAsync(user, cancellationToken);
            }

            return new UserDto(user.Id, user.Username, user.Email);
        }
    }
}
