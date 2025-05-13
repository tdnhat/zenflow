using MediatR;
using Modules.User.Domain.Interfaces;

namespace Modules.User.Features.CreateUser
{
    public class CreateUserHandler : IRequestHandler<CreateUserCommand, CreateUserResponse>
    {
        private readonly IUserRepository _userRepo;
        public CreateUserHandler(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<CreateUserResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var existingUser = await _userRepo.GetByExternalIdAsync(request.ExternalId, cancellationToken);

            if (existingUser != null)
            {
                return new CreateUserResponse
                (
                    existingUser.Id,
                    existingUser.Username,
                    existingUser.Email
                );
            }

            var user = Domain.Entities.User.Create(request.ExternalId, request.Username, request.Email);
            await _userRepo.AddAsync(user, cancellationToken);

            return new CreateUserResponse(user.Id, user.Username, user.Email);
        }
    }
}
