using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Modules.User.Services;

namespace Modules.User.Infrastructure
{
    public static class UserModuleExtensions
    {
        public static IServiceCollection AddUserModule(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();

            return services;
        }

        public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup("/api/users").WithTags("Users");

            group.MapGet("/", async (IUserService userService) =>
            {
                var users = await userService.GetAllUsersAsync();
                return Results.Ok(users);
            })
                .RequireAuthorization()
                .WithName("GetAllUsers")
                .WithOpenApi();

            group.MapGet("/{id:guid}", async (Guid id, IUserService userService) =>
            {
                var user = await userService.GetUserByIdAsync(id);
                return Results.Ok(user);
            })
                .RequireAuthorization()
                .WithName("GetUserById")
                .WithOpenApi();

            return endpoints;
        }
    }
}
