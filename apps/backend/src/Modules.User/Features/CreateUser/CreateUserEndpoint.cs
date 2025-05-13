using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ZenFlow.Shared.Application.Auth;

namespace Modules.User.Features.CreateUser
{
    public class CreateUserEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/users/", async (
                ISender sender,
                ICurrentUserService currentUser) =>
            {
                if (currentUser.UserId is null || currentUser.Email is null)
                    return Results.Unauthorized();

                var command = new CreateUserCommand(
                    currentUser.UserId, // externalId = sub (NameIdentifier)
                    currentUser.Username ?? currentUser.Email,
                    currentUser.Email
                );

                var result = await sender.Send(command);
                return Results.Created($"/{result.Id}", result);
            })
            .Produces<CreateUserResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithTags("Users")
            .WithName("Users_CreateUser")
            .RequireAuthorization();
        }
    }
}
