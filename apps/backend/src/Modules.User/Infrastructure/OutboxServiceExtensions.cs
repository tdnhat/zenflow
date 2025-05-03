using Microsoft.Extensions.DependencyInjection;
using Modules.User.DDD.Interfaces;
using Modules.User.Infrastructure.Events;
using Modules.User.Infrastructure.Outbox;

namespace Modules.User.Infrastructure
{
    public static class OutboxServiceExtensions
    {
        public static IServiceCollection AddUserOutboxServices(this IServiceCollection services)
        {
            // Register outbox repository
            services.AddScoped<IUserOutboxRepository, UserOutboxRepository>();
            
            // Register domain event service
            services.AddScoped<IUserDomainEventService, UserDomainEventService>();
            
            // Register background services
            services.AddHostedService<UserOutboxProcessor>();
            services.AddHostedService<UserOutboxCleaner>();
            
            return services;
        }
    }
}