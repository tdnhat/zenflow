using MediatR;
using Modules.User.DDD.Interfaces;
using Modules.User.Dtos;

namespace Modules.User.Features.CreateUser
{
    public class CreateUserHandler : IRequestHandler<CreateUserCommand, UserDto>
    {
        private readonly IUserRepository _userRepo;
        public CreateUserHandler(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var existingUser = await _userRepo.GetByExternalIdAsync(request.ExternalId, cancellationToken);

            if (existingUser != null)
            {
                return new UserDto
                (
                    existingUser.Id,
                    existingUser.Username,
                    existingUser.Email
                );
            }

            var user = DDD.Entities.User.Create(request.ExternalId, request.Username, request.Email);
            await _userRepo.AddAsync(user, cancellationToken);

            return new UserDto(user.Id, user.Username, user.Email);
        }
    }
}
