using MediatR;
using Modules.User.Domain.Interfaces;
using System.Threading.Tasks;

namespace Modules.User.Features.GetUserById
{
    public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, GetUserByIdResponse?>
    {
        private readonly IUserRepository _userRepository;
        public GetUserByIdHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<GetUserByIdResponse?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
            return user is null ? null : new GetUserByIdResponse(user.Id, user.Username, user.Email);
        }
    }
}
