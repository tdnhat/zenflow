using MediatR;
using Modules.User.DDD.Interfaces;
using Modules.User.Dtos;
using System.Linq;

namespace Modules.User.Features.GetUsers
{
    public class GetUsersHandler : IRequestHandler<GetUsersQuery, List<UserDto>>
    {
        private readonly IUserRepository _userRepository;
        public GetUsersHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await _userRepository.GetAllAsync(cancellationToken);
            return users.Select(users => new UserDto(users.Id, users.Username, users.Email)).ToList();
        }
    }
}
