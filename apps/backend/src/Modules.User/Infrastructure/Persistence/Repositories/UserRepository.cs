using Microsoft.EntityFrameworkCore;
using Modules.User.Domain.Interfaces;
using Modules.User.Infrastructure.Persistence;

namespace Modules.User.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserDbContext _context;

        public UserRepository(UserDbContext context)
        {
            _context = context;
        }

        public async Task<Domain.Entities.User?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.ExternalId == externalId, cancellationToken);
        }

        public async Task AddAsync(Domain.Entities.User user, CancellationToken cancellationToken = default)
        {
            await _context.Users.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<Domain.Entities.User>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Users.AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<Domain.Entities.User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        public async Task UpdateAsync(Domain.Entities.User user, CancellationToken cancellationToken = default)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Domain.Entities.User user, CancellationToken cancellationToken = default)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<Domain.Entities.User>> GetAllIncludingDeletedAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .IgnoreQueryFilters()
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<Domain.Entities.User?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        public async Task PermanentlyDeleteAsync(Domain.Entities.User user, CancellationToken cancellationToken = default)
        {
            var userId = user.Id;

            // Detach the entity if it's being tracked to prevent conflicts
            var entry = _context.Entry(user);
            if (entry.State != EntityState.Detached)
            {
                entry.State = EntityState.Detached;
            }

            // Use direct SQL to permanently delete and bypass interceptors
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM \"user\".\"Users\" WHERE \"Id\" = {userId}", cancellationToken);
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
