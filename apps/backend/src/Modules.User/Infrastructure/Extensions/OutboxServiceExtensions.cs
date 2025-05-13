using Microsoft.Extensions.DependencyInjection;
using Modules.User.Domain.Interfaces;
using Modules.User.Infrastructure.EventHandling.DomainEvents;
using Modules.User.Infrastructure.EventHandling.Outbox;
using Modules.User.Infrastructure.Persistence.Repositories;

namespace Modules.User.Infrastructure.Extensions
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