using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ZenFlow.Core.Data;
using ZenFlow.Core.Entities;

namespace ZenFlow.Core.Infrastructure
{
    public static class DatabaseExtensions
    {
        public static IServiceCollection AddApplicationDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("Api")
                );
            });

            return services;
        }
        
        // Add a method to seed the database with test data
        public static async Task SeedDatabaseAsync(this IHost app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            // Only seed if no users exist
            if (!await context.UserProfiles.AnyAsync())
            {
                var testUser = new UserProfile
                {
                    Id = Guid.NewGuid(),
                    Username = "testuser",
                    Email = "test@example.com",
                    DisplayName = "Test User",
                    AvatarUrl = "https://via.placeholder.com/150",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                
                context.UserProfiles.Add(testUser);
                await context.SaveChangesAsync();
            }
        }
    }
}
