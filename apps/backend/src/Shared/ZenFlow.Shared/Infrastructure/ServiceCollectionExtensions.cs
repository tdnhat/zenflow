using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ZenFlow.Shared.Infrastructure
{
    /// <summary>
    /// Extension methods for registering domain event services
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds MassTransit with RabbitMQ for domain event messaging
        /// </summary>
        public static IServiceCollection AddMassTransitWithRabbitMq(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMassTransit(x =>
            {
                // Auto-discover consumers from Modules
                var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => a.FullName?.StartsWith("Modules.") == true)
                    .ToList();

                foreach (var assembly in assemblies)
                {
                    x.AddConsumers(assembly);
                }

                // Configure RabbitMQ
                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitMqSection = configuration.GetSection("RabbitMQ");
                    var host = rabbitMqSection["Host"] ?? "localhost";
                    var username = rabbitMqSection["Username"] ?? "guest";
                    var password = rabbitMqSection["Password"] ?? "guest";
                    var virtualHost = rabbitMqSection["VirtualHost"] ?? "/";

                    cfg.Host(host, virtualHost, h =>
                    {
                        h.Username(username);
                        h.Password(password);
                    });

                    // Configure retry policy
                    cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));

                    // Configure topology (exchanges, queues, etc)
                    cfg.ConfigureEndpoints(context);
                });
            });

            return services;
        }
    }
}