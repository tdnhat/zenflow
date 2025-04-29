using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace ZenFlow.Shared.Extensions
{
    public static class MediatRExtension
    {
        public static IServiceCollection AddMediatRWithAssemblies(
            this IServiceCollection services, params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
            }

            return services;
        }
    }
}
