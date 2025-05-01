using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using ZenFlow.Shared.Behaviors;

namespace ZenFlow.Shared.Extensions
{
    public static class MediatRExtension
    {
        public static IServiceCollection AddMediatRWithAssemblies(
            this IServiceCollection services, params Assembly[] assemblies)
        {
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssemblies(assemblies);
                config.AddOpenBehavior(typeof(ValidationBehavior<,>));
                config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            });

            // Register all validators from the assemblies
            services.AddValidatorsFromAssemblies(assemblies);

            return services;
        }
    }
}
