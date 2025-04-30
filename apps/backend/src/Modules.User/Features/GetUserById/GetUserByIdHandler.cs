using MediatR;
using Modules.User.DDD.Interfaces;
using Modules.User.Dtos;

namespace Modules.User.Features.GetUserById
{
    public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
    {
        private readonly IUserRepository _userRepository;
        public GetUserByIdHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
            return user is null ? null : new UserDto(user.Id, user.Username, user.Email);
        }
    }
}
