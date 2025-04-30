using Microsoft.EntityFrameworkCore;
using Modules.User.Data;
using Modules.User.DDD.Interfaces;

namespace Modules.User.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserDbContext _context;

        public UserRepository(UserDbContext context)
        {
            _context = context;
        }

        public async Task<DDD.Entities.User?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.ExternalId == externalId, cancellationToken);
        }

        public async Task AddAsync(DDD.Entities.User user, CancellationToken cancellationToken = default)
        {
            await _context.Users.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<DDD.Entities.User>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Users.AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<DDD.Entities.User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        public async Task UpdateAsync(DDD.Entities.User user, CancellationToken cancellationToken = default)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(DDD.Entities.User user, CancellationToken cancellationToken = default)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<DDD.Entities.User>> GetAllIncludingDeletedAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .IgnoreQueryFilters()
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<DDD.Entities.User?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        public async Task PermanentlyDeleteAsync(DDD.Entities.User user, CancellationToken cancellationToken = default)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task RestoreAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var user = _context.Users.IgnoreQueryFilters().FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                user.IsDeleted = false;
                user.DeletedAt = null;
                user.DeletedBy = null;

                _context.Users.Update(user);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }   
}
