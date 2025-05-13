using Microsoft.Extensions.DependencyInjection;
using Modules.Workflow.Infrastructure.EventHandling.DomainEvents;
using Modules.Workflow.Infrastructure.EventHandling.Outbox;
using Modules.Workflow.Infrastructure.Outbox;

namespace Modules.Workflow.Infrastructure.Extensions
{
    public static class OutboxServiceExtensions
    {
        public static IServiceCollection AddWorkflowOutboxServices(this IServiceCollection services)
        {
            // Register outbox repository
            services.AddScoped<IWorkflowOutboxRepository, WorkflowOutboxRepository>();

            // Register domain event service
            services.AddScoped<IWorkflowDomainEventService, WorkflowDomainEventService>();

            // Register background services
            services.AddHostedService<WorkflowOutboxProcessor>();
            services.AddHostedService<WorkflowOutboxCleaner>();

            return services;
        }
    }
}