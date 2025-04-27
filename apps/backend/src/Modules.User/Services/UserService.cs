using Microsoft.EntityFrameworkCore;
using ZenFlow.Core.Data;
using ZenFlow.Shared.Models;

namespace Modules.User.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _context.UserProfiles
                .Where(u => u.IsActive)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    DisplayName = u.DisplayName ?? u.Username,
                    AvatarUrl = u.AvatarUrl
                })
                .ToListAsync();

            return users;
        }

        public Task<UserDto?> GetUserByIdAsync(Guid id)
        {
            var user = _context.UserProfiles
                .Where(u => u.IsActive && u.Id == id)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    DisplayName = u.DisplayName ?? u.Username,
                    AvatarUrl = u.AvatarUrl
                })
                .FirstOrDefaultAsync();

            return user;
        }
    }
}
