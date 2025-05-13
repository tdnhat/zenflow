using MediatR;
using Modules.User.Domain.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace Modules.User.Features.GetUsers
{
    public class GetUsersHandler : IRequestHandler<GetUsersQuery, List<GetUsersResponse>>
    {
        private readonly IUserRepository _userRepository;
        public GetUsersHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<List<GetUsersResponse>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await _userRepository.GetAllAsync(cancellationToken);
            return users.Select(user => new GetUsersResponse(user.Id, user.Username, user.Email)).ToList();
        }
    }
}
