using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Modules.Identity.Infrastructure;

public static class IdentityModuleExtensions
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        // Register Identity module services
        // ...
        
        return services;
    }
    
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/identity").WithTags("Identity");
        
        // Map Identity module endpoints
        group.MapGet("/hello", () => new { message = "Hello from Identity module!" })
             .RequireAuthorization()
             .WithName("IdentityHello")
             .WithOpenApi();
        
        return endpoints;
    }
}