using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ZenFlow.Shared.Application.Auth;
using ZenFlow.Shared.Domain;

namespace Modules.User.Data.Interceptors
{
    public class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly ICurrentUserService _currentUser;

        public AuditInterceptor(ICurrentUserService currentUser)
        {
            _currentUser = currentUser;
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context == null)
            {
                return base.SavingChangesAsync(eventData, result, cancellationToken);
            }

            var now = DateTime.UtcNow;
            var userId = _currentUser.UserId;

            foreach (var entry in eventData.Context.ChangeTracker.Entries())
            {
                // Handle audit fields
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    if (entry.State == EntityState.Added && entry.Property("CreatedAt") != null)
                    {
                        entry.Property("CreatedBy").CurrentValue = userId;
                        entry.Property("CreatedAt").CurrentValue = now;
                    }

                    if (entry.Property("LastModifiedAt") != null)
                    {
                        entry.Property("LastModifiedBy").CurrentValue = userId;
                        entry.Property("LastModifiedAt").CurrentValue = now;
                    }
                }

                // Handle soft delete
                if (entry.State == EntityState.Deleted && entry.Entity is ISoftDelete softDeleteEntity)
                {
                    // Change state to modified instead of deleted
                    entry.State = EntityState.Modified;

                    // Set soft delete properties
                    softDeleteEntity.IsDeleted = true;
                    softDeleteEntity.DeletedAt = now;
                    softDeleteEntity.DeletedBy = userId;

                    // Only update the soft delete fields
                    foreach (var property in entry.Properties)
                    {
                        if (property.Metadata.Name != nameof(ISoftDelete.IsDeleted) &&
                            property.Metadata.Name != nameof(ISoftDelete.DeletedAt) &&
                            property.Metadata.Name != nameof(ISoftDelete.DeletedBy))
                        {
                            property.IsModified = false;
                        }
                    }
                }
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}