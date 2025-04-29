using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Modules.User.Data.Configurations
{
    internal class UserConfiguration : IEntityTypeConfiguration<DDD.Entities.User>
    {
        public void Configure(EntityTypeBuilder<DDD.Entities.User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.ExternalId)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);

            builder.Ignore(u => u.DomainEvents); // Ignore domain events
        }
    }
}
