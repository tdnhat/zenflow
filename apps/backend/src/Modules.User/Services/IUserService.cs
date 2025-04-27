using ZenFlow.Core.Entities;
using ZenFlow.Shared.Models;

namespace Modules.User.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(Guid id);
    }
}
